using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using RimWorld;
using RimWorld.Planet;
using Harmony;

namespace VFEMedieval
{

    public class MedievalTournament : WorldObject
    {

        private const int GoodwillBonus = 15;
        private const float BaseXPGain = 6000;
        

        public override string Label => $"{base.Label} ({category.ToStringHuman()})";

        private TimeoutComp TimeoutComp => GetComponent<TimeoutComp>();

        public override Material Material
        {
            get
            {
                if (cachedMat == null)
                {
                    cachedMat = MaterialPool.MatFrom(def.texture, ShaderDatabase.WorldOverlayTransparentLit, Color.white, WorldMaterials.WorldObjectRenderQueue);
                }
                return cachedMat;
            }
        }

        public void Notify_CaravanArrived(Caravan caravan)
        {
            var participants = new List<Pawn>();
            var nonParticipants = new List<Pawn>();
            MedievalTournamentUtility.GroupParticipants(caravan.PlayerPawnsForStoryteller.Where(p => p.RaceProps.Humanlike), category, participants, nonParticipants);

            // Create dialogue tree
            var leader = Faction.leader;
            var tourneyNode = new DiaNode("VanillaFactionsExpanded.MedievalTournamentInitial".Translate(leader.LabelShort, Faction.Name, category.ToStringHuman(), competitorCount, GenLabel.ThingsLabel(rewards), leader.Named("PAWN")));

            // Option 1: Participate
            var participateNode = new DiaNode("VanillaFactionsExpanded.ParticipateInitial".Translate());
            var participateOption = new DiaOption("VanillaFactionsExpanded.Participate".Translate())
            {
                link = participateNode
            };
            tourneyNode.options.Add(participateOption);
            foreach (var pawn in participants)
            {
                var pawnOption = new DiaOption(MedievalTournamentUtility.ParticipantOptionText(pawn, category))
                {
                    action = () => DoTournament(caravan, pawn),
                    resolveTree = true
                };
                participateNode.options.Add(pawnOption);
            }
            foreach (var pawn in nonParticipants)
            {
                var pawnOption = new DiaOption(MedievalTournamentUtility.ParticipantOptionText(pawn, category))
                {
                    disabled = true
                };
                participateNode.options.Add(pawnOption);
            }
            var participateGoBack = new DiaOption("GoBack".Translate())
            {
                link = tourneyNode
            };
            participateNode.options.Add(participateGoBack);

            // Option 2: Attack (angers faction)
            var attackOption = new DiaOption($"{"CommandAttackSettlement".Translate()} ({"AngersFaction".Translate()})")
            {
                action = () =>
                {
                    LongEventHandler.QueueLongEvent(() =>
                    {
                        Faction.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Hostile, false);
                        var competitorPool = PossibleCompetitors.ToList();
                        var extraPawnParams = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, caravan);
                        extraPawnParams.faction = Faction;
                        var pawnGroupMakerParams = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, extraPawnParams, true);
                        pawnGroupMakerParams.generateFightersOnly = true;
                        var hostilePawns = MedievalTournamentUtility.GenerateCompetitors(competitorCount, category, Faction, PossibleCompetitors).Concat(PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParams)).ToList();
                        var map = CaravanIncidentUtility.SetupCaravanAttackMap(caravan, hostilePawns, true);
                        if (hostilePawns.Any())
                            LordMaker.MakeNewLord(Faction, new LordJob_AssaultColony(Faction), map, hostilePawns);
                        Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                        foreach (var thing in rewards)
                            GenPlace.TryPlaceThing(thing, map.Center, map, ThingPlaceMode.Near);
                    },
                    "GeneratingMapForNewEncounter", false, null);
                    Find.WorldObjects.Remove(this);
                },
                resolveTree = true
            };
            tourneyNode.options.Add(attackOption);

            // Option 3: Leave
            var leaveOption = new DiaOption("VanillaFactionsExpanded.Leave".Translate())
            {
                resolveTree = true
            };
            tourneyNode.options.Add(leaveOption);

            // Add dialogue menu
            Find.WindowStack.Add(new Dialog_NodeTree(tourneyNode, title: LabelCap));
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (var o in base.GetFloatMenuOptions(caravan))
                yield return o;

            foreach (var o in CaravanArrivalAction_AttendMedievalTournament.GetFloatMenuOptions(caravan, this))
                yield return o;
        }

        private IEnumerable<Pawn> PossibleCompetitors => Find.WorldPawns.GetPawnsBySituation(WorldPawnSituation.Free).Where(p => p.Faction == Faction);

        private void DoTournament(Caravan caravan, Pawn participant)
        {
            var competitorPool = MedievalTournamentUtility.GenerateCompetitors(competitorCount, category, Faction, PossibleCompetitors);
            var disasterPool = competitorPool.Concat(caravan.PawnsListForReading).ToList();
            float participantEff = MedievalTournamentUtility.TournamentEffectivenessFor(participant, category);

            // Simulate a progression through the rounds
            int curPlace = competitorCount + 1;
            for (int i = 0; i < competitorCount; i++)
            {
                Pawn competitor = competitorPool.RandomElement();
                competitorPool.Remove(competitor);

                // This is where battle simulation happens
                float competitorEff = MedievalTournamentUtility.TournamentEffectivenessFor(competitor, category);
                float winChance = MedievalTournamentUtility.EffectivenessAdvantageToWinChanceCurve.Evaluate(participantEff - competitorEff);
                if (!ResolveDisaster(participant, competitor, disasterPool, winChance))
                {
                    if (Rand.Chance(winChance))
                        curPlace--;
                    else
                        break;
                }
                else
                {
                    Find.WorldObjects.Remove(this);
                    return;
                }
            }
            ResolveOutcome(participant, curPlace, caravan);
            Find.WorldObjects.Remove(this);
        }

        private bool ResolveDisaster(Pawn participant, Pawn competitor, List<Pawn> pool, float winChance)
        {
            float partChance = (1 - winChance) / 20;
            float compChance = winChance / 20;
            switch (category)
            {
                case TournamentCategory.Melee:
                    return ResolveDisaster_Melee(participant, competitor, pool, partChance, compChance);
                case TournamentCategory.Jousting:
                    return ResolveDisaster_Jousting(participant, competitor, pool, partChance, compChance);
                case TournamentCategory.Archery:
                    return ResolveDisaster_Archery(participant, competitor, pool, partChance, compChance);
                default:
                    throw new NotImplementedException();
            }
        }

        private bool ResolveDisaster_Melee(Pawn participant, Pawn competitor, List<Pawn> pool, float partChance, float compChance)
        {

        }

        private bool ResolveDisaster_Jousting(Pawn participant, Pawn competitor, List<Pawn> pool, float partChance, float compChance)
        {

        }

        private bool ResolveDisaster_Archery(Pawn participant, Pawn competitor, List<Pawn> pool, float partChance, float compChance)
        {

        }

        private void ResolveOutcome(Pawn participant, int place, Caravan caravan)
        {
            string placeOrdinal = Find.ActiveLanguageWorker.OrdinalNumber(place);
            string totalOrdinal = Find.ActiveLanguageWorker.OrdinalNumber(competitorCount + 1);
            string letterLabel;
            string letterTextMain;
            LetterDef letterDef;
            if (place == 1)
            {
                letterLabel = "VanillaFactionsExpanded.TournamentWinLetter".Translate();
                letterTextMain = "VanillaFactionsExpanded.TournamentWinLetter_Text".Translate(placeOrdinal, totalOrdinal, Faction, GenLabel.ThingsLabel(rewards), participant.Named("PAWN"));
                letterDef = LetterDefOf.PositiveEvent;

                Faction.TryAffectGoodwillWith(Faction.OfPlayer, GoodwillBonus);
                for (int i = 0; i < rewards.Count; i++)
                    caravan.AddPawnOrItem(rewards[i], true);
            }
            else
            {
                letterLabel = "VanillaFactionsExpanded.TournamentLossLetter".Translate();
                letterTextMain = "VanillaFactionsExpanded.TournamentLossLetter_Text".Translate(placeOrdinal, totalOrdinal, participant.Named("PAWN"));
                letterDef = LetterDefOf.NeutralEvent;
            }
            ResolveXPGains(participant, out string xpText);
            letterTextMain += $"\n\n{xpText}";
            Find.LetterStack.ReceiveLetter(letterLabel, letterTextMain, letterDef);
        }

        private void ResolveXPGains(Pawn pawn, out string xpText)
        {
            string baseTextKey = $"VanillaFactionsExpanded.TournamentXPGain_{category}";
            switch (category)
            {
                case TournamentCategory.Melee:
                    pawn.skills.Learn(SkillDefOf.Melee, BaseXPGain, true);
                    xpText = baseTextKey.Translate(BaseXPGain, pawn.Named("PAWN"));
                    return;
                case TournamentCategory.Jousting:
                    float xpGain = BaseXPGain / 2;
                    pawn.skills.Learn(SkillDefOf.Melee, xpGain, true);
                    pawn.skills.Learn(SkillDefOf.Animals, xpGain, true);
                    xpText = baseTextKey.Translate(xpGain, pawn.Named("PAWN"));
                    return;
                case TournamentCategory.Archery:
                    pawn.skills.Learn(SkillDefOf.Shooting, BaseXPGain, true);
                    xpText = baseTextKey.Translate(BaseXPGain, pawn.Named("PAWN"));
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        public void GenerateRewards()
        {
            var parms = new ThingSetMakerParams()
            {
                techLevel = Faction?.def.techLevel ?? (TechLevel)Rand.RangeInclusive(1, 7)
            };
            rewards = ThingSetMakerDefOf.VFEM_Reward_MedievalTournament.root.Generate(parms);
        }

        public override string GetInspectString()
        {
            var inspectBuilder = new StringBuilder();
            inspectBuilder.AppendLine(base.GetInspectString());
            inspectBuilder.AppendLine($"VanillaFactionsExpanded.MedievalTournamentInspectString".Translate(GenThing.ThingsToCommaList(rewards), TimeoutComp.TicksLeft.ToStringTicksToDays()));
            return inspectBuilder.ToString().TrimEndNewlines();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref category, "category");
            Scribe_Values.Look(ref competitorCount, "competitorCount");
            Scribe_Collections.Look(ref rewards, "rewards", LookMode.Deep);
            base.ExposeData();
        }

        public TournamentCategory category;
        public int competitorCount;
        public List<Thing> rewards;

        private Material cachedMat;

    }

}
