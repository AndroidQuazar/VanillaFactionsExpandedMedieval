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
using HarmonyLib;
using VFECore;

namespace VFEMedieval
{

    public class SymbolResolver_CastleEdgeSandbags : SymbolResolver
    {

        private static readonly IntRange LineLengthRange = new IntRange(2, 3);
        private static readonly IntRange GapLengthRange = new IntRange(3, 4);

        public static ThingDef DefToUse
        {
            get
            {
                // VFE Security integration
                if (ModCompatibilityCheck.VanillaFurnitureExpandedSecurity)
                    return ThingDefNamed.VFES_ShortWall;

                return RimWorld.ThingDefOf.Sandbags;
            }
        }

        public override void Resolve(ResolveParams rp)
        {
            var map = BaseGen.globalSettings.map;
            var faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();
            var medievalrp = rp.GetCustom<VFEResolveParams>(VFEResolveParams.Name);
            float towerRadius = medievalrp?.towerRadius ?? 0;
            bool symmetrical = medievalrp?.symmetricalSandbags ?? Rand.Bool;

            // North
            GenerateSandbags(Rot4.North, rp, map, faction, towerRadius, symmetrical);

            // East
            GenerateSandbags(Rot4.East, rp, map, faction, towerRadius, symmetrical);

            // South
            GenerateSandbags(Rot4.South, rp, map, faction, towerRadius, symmetrical);

            // West
            GenerateSandbags(Rot4.West, rp, map, faction, towerRadius, symmetrical);
        }

        private void GenerateSandbags(Rot4 direction, ResolveParams rp, Map map, Faction faction, float towerRadius, bool symmetrical)
        {
            int towerRadInt = (int)towerRadius;
            var potentialCells = rp.rect.GetEdgeCells(direction);

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
                    if (!rp.chanceToSkipWallBlock.HasValue || !Rand.Chance(rp.chanceToSkipWallBlock.Value))
                        TrySpawnSandbags(potentialCellList[i], map, faction, rp.wallStuff);
                    if (symmetrical && (!rp.chanceToSkipWallBlock.HasValue || !Rand.Chance(rp.chanceToSkipWallBlock.Value)))
                        TrySpawnSandbags(potentialCellList[potentialCellList.Count - i], map, faction, rp.wallStuff);
                    lineCellsToDo--;
                }
                else if (gapCellsToDo > 0)
                {
                    gapCellsToDo--;
                }
            }
        }

        private void TrySpawnSandbags(IntVec3 c, Map map, Faction faction, ThingDef stuff)
        {
            if (!c.InBounds(map))
                return;

            if (TryClearCell(c, map))
            {
                // Try and make it buildable
                if (!GenConstruct.CanBuildOnTerrain(DefToUse, c, map, Rot4.North))
                {
                    if (GenConstruct.CanBuildOnTerrain(TerrainDefOf.PavedTile, c, map, Rot4.North))
                        map.terrainGrid.SetTerrain(c, TerrainDefOf.PavedTile);
                    else if (GenConstruct.CanBuildOnTerrain(TerrainDefOf.Bridge, c, map, Rot4.North))
                        map.terrainGrid.SetTerrain(c, TerrainDefOf.Bridge);
                }

                if (!GenConstruct.CanBuildOnTerrain(DefToUse, c, map, Rot4.North))
                    return;

                var usedStuff = DefToUse.MadeFromStuff ? (stuff ?? GenStuff.DefaultStuffFor(DefToUse)) : null;
                Thing thing = ThingMaker.MakeThing(DefToUse, usedStuff);
                thing.SetFaction(faction, null);
                GenSpawn.Spawn(thing, c, map, WipeMode.Vanish);
            }
        }

        private bool TryClearCell(IntVec3 c, Map map)
        {
            var thingList = c.GetThingList(map);
            for (int i = 0; i < thingList.Count; i++)
            {
                var thing = thingList[i];
                if (!thing.def.destroyable || thing.def == RimWorld.ThingDefOf.Sandbags || (thing.def.building != null && thing.def.building.isNaturalRock))
                    return false;
            }

            for (int i = 0; i < thingList.Count; i++)
                thingList[i].Destroy();
            return true;
        }

    }

}
