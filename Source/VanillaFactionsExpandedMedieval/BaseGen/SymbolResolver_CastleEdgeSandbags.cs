using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using RimWorld.BaseGen;
using Harmony;

namespace VFEMedieval
{

    public class SymbolResolver_CastleEdgeSandbags : SymbolResolver
    {

        private static readonly IntRange LineLengthRange = new IntRange(2, 3);
        private static readonly IntRange GapLengthRange = new IntRange(3, 4);

        public override void Resolve(ResolveParams rp)
        {
            var map = BaseGen.globalSettings.map;
            var faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();
            var medievalrp = rp.GetCustom<MedievalResolveParams>(MedievalResolveParams.Name);
            float towerRadius = medievalrp?.towerRadius ?? 0;
            bool symmetrical = medievalrp?.symmetricalSandbags ?? Rand.Bool;

            // North
            GenerateSandbags(Rot4.North, rp.rect, map, faction, towerRadius, symmetrical);

            // East
            GenerateSandbags(Rot4.East, rp.rect, map, faction, towerRadius, symmetrical);

            // South
            GenerateSandbags(Rot4.South, rp.rect, map, faction, towerRadius, symmetrical);

            // West
            GenerateSandbags(Rot4.West, rp.rect, map, faction, towerRadius, symmetrical);
        }

        private void GenerateSandbags(Rot4 direction, CellRect rect, Map map, Faction faction, float towerRadius, bool symmetrical)
        {
            int towerRadInt = (int)towerRadius;
            var potentialCells = rect.GetEdgeCells(direction);

            // Make sure we actually have enough cells to work with for sandbag placement
            if (towerRadInt * 2 + 2 >= potentialCells.Count())
                return;

            var potentialCellList = potentialCells.Take(potentialCells.Count() - (towerRadInt + 2)).Skip(towerRadInt + 1).ToList();
            int gapCellsToDo = 0;
            int lineCellsToDo = 0;
            for (int i = 1; i < potentialCellList.Count / (symmetrical ? 2 : 1); i++)
            {
                if (lineCellsToDo == 0 && gapCellsToDo == 0)
                {
                    lineCellsToDo = LineLengthRange.RandomInRange;
                    gapCellsToDo = GapLengthRange.RandomInRange;
                }
                if (lineCellsToDo > 0)
                {
                    TrySpawnSandbags(potentialCellList[i], map, faction);
                    if (symmetrical)
                        TrySpawnSandbags(potentialCellList[potentialCellList.Count - i], map, faction);
                    lineCellsToDo--;
                }
                else if (gapCellsToDo > 0)
                {
                    gapCellsToDo--;
                }
            }
        }

        private void TrySpawnSandbags(IntVec3 c, Map map, Faction faction)
        {
            if (!c.InBounds(map))
                return;

            if (TryClearCell(c, map))
            {
                Thing thing = ThingMaker.MakeThing(RimWorld.ThingDefOf.Sandbags, null);
                thing.SetFaction(faction, null);
                GenSpawn.Spawn(thing, c, map, WipeMode.Vanish);
            }
        }

        private bool TryClearCell(IntVec3 c, Map map)
        {
            var thingList = c.GetThingList(map);
            foreach (var thing in thingList)
            {
                if (!thing.def.destroyable || thing.def == RimWorld.ThingDefOf.Sandbags)
                    return false;
            }

            for (int i = 0; i < thingList.Count; i++)
                thingList[i].Destroy();
            return true;
        }

    }

}
