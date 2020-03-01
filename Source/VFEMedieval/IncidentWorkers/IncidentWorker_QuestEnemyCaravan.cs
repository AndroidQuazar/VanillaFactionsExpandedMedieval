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

    public class IncidentWorker_QuestEnemyCaravan : IncidentWorker
    {

        private const int MaxSettlementDistFromPlayerSettlement = 100;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            return Find.FactionManager.RandomEnemyFaction(allowNonHumanlike: false) is Faction f && TryFindSettlement(null, f, out var s1) && TryFindSettlement(s1, f, out var s2);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var faction = parms.faction ?? Find.FactionManager.RandomEnemyFaction(allowNonHumanlike: false);

            // No faction
            if (faction == null)
                return false;

            // No start point or destination
            if (!TryFindSettlement(null, faction, out var startSettlement) || !TryFindSettlement(startSettlement, faction, out var destination))
                return false;

            var caravan = EnemyCaravanUtility.MakeCaravan(faction, startSettlement.Tile, destination.Tile);
            Find.WorldObjects.Add(caravan);
            //string letterText = def.letterText.Formatted();
            Find.LetterStack.ReceiveLetter(def.letterLabel, def.letterText, def.letterDef, caravan, faction);
            return true;
        }

        private bool TryFindSettlement(SettlementBase start, Faction faction, out SettlementBase result)
        {
            // Build settlement lists
            var playerSettlements = new List<SettlementBase>();
            var validEnemyAlliedSettlements = new List<SettlementBase>();
            var settlementBases = Find.WorldObjects.SettlementBases;
            for (int i = 0; i < settlementBases.Count; i++)
            {
                var settlement = settlementBases[i];
                if (settlement.Faction == Faction.OfPlayer)
                    playerSettlements.Add(settlement);
                else if (settlement != start && (settlement.Faction == faction || !settlement.Faction.HostileTo(faction)))
                    validEnemyAlliedSettlements.Add(settlement);
            }
            var playerSettlement = playerSettlements.RandomElement();

            // Return result
            if (start != null)
            {
                var worldGrid = Find.WorldGrid;
                var dirToPlayer = worldGrid.GetDirection8WayFromTo(start.Tile, playerSettlement.Tile);
                return validEnemyAlliedSettlements.TryRandomElementByWeight(s =>
                {
                    var dirToDest = worldGrid.GetDirection8WayFromTo(start.Tile, s.Tile);
                    float weightMult = dirToPlayer == dirToDest ? 10 : 1;
                    return 1f / worldGrid.TraversalDistanceBetween(playerSettlement.Tile, s.Tile, false) * weightMult;
                }, out result);
            }
            return validEnemyAlliedSettlements.TryRandomElementByWeight(s => 1f / Find.WorldGrid.TraversalDistanceBetween(playerSettlement.Tile, s.Tile, false), out result);
        }

    }

}
