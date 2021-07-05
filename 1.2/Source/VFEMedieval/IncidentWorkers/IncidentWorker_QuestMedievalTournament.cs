using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.Planet;
using Verse.AI.Group;

namespace VFEMedieval
{

    public class IncidentWorker_QuestMedievalTournament : IncidentWorker
    {

        private const int MaxDistFromPlayerWorldObject = 50;
        private static readonly IntRange QuestSiteDistanceRange = new IntRange(2, 4);


        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && TryFindFaction(out var f) && TryFindTile(f, out int t, out string s);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var hostFaction = parms.faction;

            // No faction
            if (hostFaction == null && !TryFindFaction(out hostFaction))
                return false;

            // No tile
            if (!TryFindTile(hostFaction, out int tile, out string settlementName))
                return false;

            var tournament = (MedievalTournament)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.VFEM_MedievalTournament);
            tournament.Tile = tile;
            tournament.SetFaction(hostFaction);
            var tournamentCategory = DefDatabase<TournamentCategoryDef>.AllDefsListForReading.RandomElementByWeight(c => c.commonality);
            tournament.category = tournamentCategory;
            tournament.competitorCount = MedievalTournamentUtility.CompetitorCountRange.RandomInRange;
            int durationDays = MedievalTournamentUtility.QuestSiteTournamentTimeoutDaysRange.RandomInRange;
            tournament.GetComponent<TimeoutComp>().StartTimeout(durationDays * GenDate.TicksPerDay);
            tournament.GenerateRewards();
            Find.WorldObjects.Add(tournament);

            Find.LetterStack.ReceiveLetter(def.letterLabel, GetLetterText(settlementName, tournament, durationDays), def.letterDef, tournament, hostFaction);

            return true;
        }

        private string GetLetterText(string settlementName, MedievalTournament tournament, int durationDays)
        {
            var hostFaction = tournament.Faction;
            var textBuilder = new StringBuilder();
            textBuilder.AppendLine(def.letterText.Formatted(hostFaction.def.leaderTitle, hostFaction.Name, settlementName, GenLabel.ThingsLabel(tournament.rewards), hostFaction.leader.Named("PAWN")).AdjustedFor(hostFaction.leader));
            textBuilder.AppendLine();
            textBuilder.AppendLine(tournament.category.description);
            textBuilder.AppendLine();
            textBuilder.AppendLine("VanillaFactionsExpanded.TournamentDurationDays".Translate(durationDays));
            return textBuilder.ToString().TrimEndNewlines();
        }

        private bool TryFindFaction(out Faction faction)
        {
            return Find.FactionManager.AllFactions.Where(f => !f.IsPlayer && f.def.techLevel == TechLevel.Medieval && !f.defeated && !f.def.hidden && !TournamentExists(f) && !f.HostileTo(Faction.OfPlayer)).TryRandomElement(out faction);
        }

        private bool TryFindTile(Faction hostFaction, out int tile, out string settlementName)
        {
            // Try to find an eligible settlement to host the tournament and then a tile near that settlement
            var eligibleSettlements = Find.WorldObjects.SettlementBases.Where(s => s.Faction == hostFaction);
            if (Find.WorldObjects.AllWorldObjects.Where(f => f.Faction == Faction.OfPlayer && (f is Caravan || f is MapParent)).TryRandomElement(out var playerObject))
            {
                int ourTile = playerObject.Tile;
                var worldGrid = Find.WorldGrid;
                eligibleSettlements = eligibleSettlements.Where(s => worldGrid.TraversalDistanceBetween(ourTile, s.Tile, false) < MaxDistFromPlayerWorldObject);
                if (eligibleSettlements.TryRandomElementByWeight(s => 1f / worldGrid.TraversalDistanceBetween(ourTile, s.Tile, false), out var targetSettlement))
                {
                    settlementName = targetSettlement.Label;
                    return TileFinder.TryFindNewSiteTile(out tile, QuestSiteDistanceRange.min, QuestSiteDistanceRange.max, true, false, targetSettlement.Tile);
                }
                    
            }
            tile = -1;
            settlementName = null;
            return false;
        }

        private bool TournamentExists(Faction faction)
        {
            return Find.WorldObjects.AllWorldObjects.Any(w => w.def == WorldObjectDefOf.VFEM_MedievalTournament && w.Faction == faction);
        }

    }

}
