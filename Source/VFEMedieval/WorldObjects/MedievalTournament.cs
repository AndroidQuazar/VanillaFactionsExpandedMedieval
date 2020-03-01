using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;

namespace VFEMedieval
{

    public class MedievalTournament : WorldObject
    {

        private const int GoodwillBonus = 15;
        private const float BaseXPGain = 6000;
        

        public override string Label => $"{base.Label} ({category.label})";

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
            MedievalTournamentUtility.GroupParticipants(caravan.PlayerPawnsForStoryteller.Where(p => p.RaceProps.Humanlike).ToList(), category, participants, nonParticipants);

            // Create dialogue tree
            var leader = Faction.leader;
            var tourneyNode = new DiaNode("VanillaFactionsExpanded.MedievalTournamentInitial".Translate(leader.LabelShort, Faction.Name, category.label, competitorCount, GenLabel.ThingsLabel(rewards), leader.Named("PAWN")));

            // Option 1: Participate
            var participateNode = new DiaNode("VanillaFactionsExpanded.ParticipateInitial".Translate());
            var participateOption = new DiaOption("VanillaFactionsExpanded.Participate".Translate())
            {
                link = participateNode
            };
            tourneyNode.options.Add(participateOption);
            for (int i = 0; i < participants.Count; i++)
            {
                var pawn = participants[i];
                var pawnOption = new DiaOption(MedievalTournamentUtility.ParticipantOptionText(pawn, category))
                {
                    action = () => DoTournament(caravan, pawn),
                    resolveTree = true
                };
                participateNode.options.Add(pawnOption);
            }
            for (int i = 0; i < nonParticipants.Count; i++)
            {
                var pawn = nonParticipants[i];
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
                        for (int i = 0; i < rewards.Count; i++)
                            GenPlace.TryPlaceThing(rewards[i], map.Center, map, ThingPlaceMode.Near);
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
            var fullAudience = competitorPool.Concat(caravan.PawnsListForReading).Where(p => p != participant).ToList();
            float participantEff = MedievalTournamentUtility.TournamentEffectivenessFor(participant, category);
            var entries = new List<BattleLogEntry_Event>();
            bool cancelled = false;

            // Simulate a progression through the rounds
            int curPlace = competitorCount + 1;
            for (int i = 0; i < competitorCount; i++)
            {
                var competitor = competitorPool.RandomElement();
                competitorPool.Remove(competitor);

                // This is where battle simulation happens
                float competitorEff = MedievalTournamentUtility.TournamentEffectivenessFor(competitor, category);
                float winChance = MedievalTournamentUtility.EffectivenessAdvantageToWinChanceCurve.Evaluate(participantEff - competitorEff);
                var victor = Rand.Chance(winChance) ? participant : competitor;
                var loser = (participant == victor) ? competitor : participant;
                if (!ResolveDisaster(victor, loser, fullAudience.Where(p => p != competitor).ToList(), out var victim, out var disEntry))
                {
                    if (category.rulePack != null)
                        entries.Add(new BattleLogEntry_Event(loser, category.rulePack, victor));
                    if (participant == victor)
                        curPlace--;
                    else
                        break;
                }
                else
                {
                    if (disEntry != null)
                        entries.Add(disEntry);
                    cancelled = true;
                    break;
                }
            }
            ResolveOutcome(participant, entries, curPlace, cancelled, caravan);
            Find.WorldObjects.Remove(this);
        }

        private bool ResolveDisaster(Pawn victor, Pawn loser, List<Pawn> audience, out Pawn victim, out BattleLogEntry_Event entry)
        {
            victim = null;
            entry = null;

            // Disaster doesn't happen
            if (!Rand.Chance(MedievalTournamentUtility.DisasterChancePerRound))
                return false;

            // Resolve disaster
            if (category == TournamentCategoryDefOf.VFEM_Melee)
                ResolveDisaster_Melee(victor, loser, audience, ref victim, ref entry);
            else if (category == TournamentCategoryDefOf.VFEM_Jousting)
                ResolveDisaster_Jousting(victor, loser, audience, ref victim, ref entry);
            else if (category == TournamentCategoryDefOf.VFEM_Archery)
                ResolveDisaster_Archery(victor, loser, audience, ref victim, ref entry);
            else
                throw new NotImplementedException();

            return true;
        }

        private void ResolveDisaster_Melee(Pawn victor, Pawn loser, List<Pawn> audience, ref Pawn victim, ref BattleLogEntry_Event entry)
        {
            // Apply injury
            victim = loser;
            var randValue = Rand.Value;

            // 90% chance: injury
            if (randValue <= 0.9f)
            {
                var damResult = victim.TakeDamage(new DamageInfo(RimWorld.DamageDefOf.Cut, Rand.RangeInclusive(14, 22), weapon: ThingDefOf.MeleeWeapon_Gladius));

                // 33% chance: will scar
                if (Rand.Chance(0.33f))
                {
                    for (int i = 0; i < damResult.hediffs.Count; i++)
                    {
                        var hediff = damResult.hediffs[i];
                        if (hediff.TryGetComp<HediffComp_GetsPermanent>() is HediffComp_GetsPermanent scarComp && !scarComp.IsPermanent && scarComp.permanentDamageThreshold == 9999)
                            scarComp.permanentDamageThreshold = Rand.Range(1, hediff.Severity / 2);
                    }
                }

                entry = new BattleLogEntry_Event(victim, RulePackDefOf.VFEM_Event_MeleeDisasterCut, null);
            }

            // 10% chance: missing part (try not to kill)
            else
            {
                int i = 0;
                var part = victim.health.hediffSet.GetRandomNotMissingPart(RimWorld.DamageDefOf.Cut);
                while (victim.health.WouldDieAfterAddingHediff(HediffDefOf.MissingBodyPart, part, 0.01f))
                {
                    if (i >= 1000)
                    {
                        Log.Error($"Tried to do non-lethal part removal for {victim} but failed after 1000 attempts.");
                        return;
                    }
                    part = victim.health.hediffSet.GetRandomNotMissingPart(RimWorld.DamageDefOf.Cut);
                    i++;
                }
                victim.health.AddHediff(RimWorld.HediffDefOf.MissingBodyPart, part);
                entry = new BattleLogEntry_Event(victim, RulePackDefOf.VFEM_Event_MeleeDisasterPartLoss, null);
            }
        }

        private void ResolveDisaster_Jousting(Pawn victor, Pawn loser, List<Pawn> audience, ref Pawn victim, ref BattleLogEntry_Event entry)
        {
            var randValue = Rand.Value;

            // 50% chance: Apply standard melee disaster
            if (randValue <= 0.5f)
                ResolveDisaster_Melee(victor, loser, audience, ref victim, ref entry);

            // 50% chance: Horse derp
            else
            {
                victim = loser;
                victim.TakeDamage(new DamageInfo(RimWorld.DamageDefOf.Blunt, Rand.Range(12, 20)));
                entry = new BattleLogEntry_Event(victim, RulePackDefOf.VFEM_Event_JoustingDisaster, null);
            }
        }

        private void ResolveDisaster_Archery(Pawn victor, Pawn loser, List<Pawn> audience, ref Pawn victim, ref BattleLogEntry_Event entry)
        {
            float randValue = Rand.Value;

            // 60% chance: severe injury to participant or competitor
            if (randValue <= 0.6f)
            {
                victim = loser;
                victim.TakeDamage(new DamageInfo(RimWorld.DamageDefOf.Cut, Rand.RangeInclusive(10, 14)));
                entry = new BattleLogEntry_Event(victim, RulePackDefOf.VFEM_Event_ArcheryDisaster, null);
            }

            // 40% chance: random audience pawn gets hit
            else
            {
                victim = audience.RandomElement();
                victim.TakeDamage(new DamageInfo(RimWorld.DamageDefOf.Arrow, 17, 0.15f, weapon: ThingDefOf.Bow_Great));
                victim.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -15);
                entry = new BattleLogEntry_Event(victim, RulePackDefOf.VFEM_Event_ArcheryDisasterAudience, loser);
            }
       }

        private void ResolveOutcome(Pawn participant, List<BattleLogEntry_Event> entries, int place, bool cancelled, Caravan caravan)
        {
            string placeOrdinal = Find.ActiveLanguageWorker.OrdinalNumber(place);
            string totalOrdinal = Find.ActiveLanguageWorker.OrdinalNumber(competitorCount + 1);
            string letterLabel;
            var letterTextBuilder = new StringBuilder();
            LetterDef letterDef;
            if (!cancelled || place == 1)
            {
                if (place == 1)
                {
                    letterLabel = "VanillaFactionsExpanded.TournamentWinLetter".Translate();
                    letterTextBuilder.AppendLine("VanillaFactionsExpanded.TournamentWinLetter_Text".Translate(placeOrdinal, totalOrdinal, Faction, GenLabel.ThingsLabel(rewards), participant.Named("PAWN")));
                    letterDef = LetterDefOf.PositiveEvent;

                    // Give rewards
                    Faction.TryAffectGoodwillWith(Faction.OfPlayer, GoodwillBonus);
                    for (int i = 0; i < rewards.Count; i++)
                        caravan.AddPawnOrItem(rewards[i], true);
                }
                else
                {
                    letterLabel = "VanillaFactionsExpanded.TournamentLossLetter".Translate();
                    letterTextBuilder.AppendLine("VanillaFactionsExpanded.TournamentLossLetter_Text".Translate(placeOrdinal, totalOrdinal, participant.Named("PAWN")));
                    letterDef = LetterDefOf.NeutralEvent;
                }
            }
            else
            {
                letterLabel = "VanillaFactionsExpanded.TournamentCancelledLetter".Translate();
                letterTextBuilder.AppendLine("VanillaFactionsExpanded.TournamentCancelledLetter_Text".Translate());
                letterDef = LetterDefOf.NegativeEvent;
            }

            // XP gains
            ResolveXPGains(participant, out string xpText);
            if (xpText != null)
            {
                letterTextBuilder.AppendLine();
                letterTextBuilder.AppendLine(xpText);
            }

            // Add log entries
            if (entries.Any())
            {
                letterTextBuilder.AppendLine();
                for (int i = 0; i < entries.Count; i++)
                {
                    var curEntry = entries[i];
                    letterTextBuilder.AppendLine("VanillaFactionsExpanded.LogEntryLine".Translate(i + 1, curEntry.ToGameStringFromPOV(null)));
                    Find.PlayLog.Add(curEntry);
                }
            }

            // Add letter to letter stack
            Find.LetterStack.ReceiveLetter(letterLabel, letterTextBuilder.ToString().TrimEndNewlines(), letterDef);
        }

        private void ResolveXPGains(Pawn pawn, out string xpText)
        {
            if (category.skillExpGains != null && category.skillExpGains.Any())
            {
                var expReadouts = new List<string>();
                foreach (var gain in category.skillExpGains)
                {
                    pawn.skills.Learn(gain.Key, gain.Value, true);
                    expReadouts.Add($"{gain.Value} {gain.Key.label}");
                }
                xpText = "VanillaFactionsExpanded.TournamentExpGains".Translate(expReadouts.ToCommaList(true), pawn.Named("PAWN"));
            }
            else
                xpText = null;
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
            Scribe_Defs.Look(ref category, "category");
            Scribe_Values.Look(ref competitorCount, "competitorCount");
            Scribe_Collections.Look(ref rewards, "rewards", LookMode.Deep);
            base.ExposeData();
        }

        public TournamentCategoryDef category;
        public int competitorCount;
        public List<Thing> rewards;

        private Material cachedMat;

    }

}
