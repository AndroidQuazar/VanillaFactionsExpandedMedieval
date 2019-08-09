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

    public class SymbolResolver_CastleEdgeWalls : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            var perimeterWallCells = rp.rect.EdgeCells;
            var map = BaseGen.globalSettings.map;
            var faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();

            // Generate towers on each corner of the rect
            var corners = rp.rect.Corners;
            float towerRadius = rp.GetCustom<MedievalResolveParams>(MedievalResolveParams.Name)?.towerRadius ?? 3.9f;
            var wallStuff = rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(faction, true);
            foreach (var corner in corners)
            {
                var towerCells = GenRadial.RadialCellsAround(corner, towerRadius, true);
                perimeterWallCells = perimeterWallCells.Where(c => !towerCells.Contains(c));
                var towerInteriorCells = GenRadial.RadialCellsAround(corner, towerRadius - 1.42f, true);
                foreach (var pos in towerCells)
                    DoTowerSection(pos, map, rp, wallStuff, towerInteriorCells);
            }

            // Generate perimeter walls
            foreach (var pos in perimeterWallCells)
                TrySpawnWall(pos, map, rp, wallStuff);
        }

        private void DoTowerSection(IntVec3 c, Map map, ResolveParams rp, ThingDef wallStuff, IEnumerable<IntVec3> interiorCells)
        {
            // Not in bounds
            if (!c.InBounds(map))
                return;

            // Walls
            if (!interiorCells.Contains(c))
                TrySpawnWall(c, map, rp, wallStuff);

            // Interior
            else if (TryClearCell(c, map))
            {
                var floorDef = GenConstruct.CanBuildOnTerrain(TerrainDefOf.WoodPlankFloor, c, map, Rot4.North) ? TerrainDefOf.WoodPlankFloor : TerrainDefOf.Bridge;
                map.terrainGrid.SetTerrain(c, floorDef);
            }

            // Roof
            if (rp.noRoof != false)
                TrySpawnRoof(c, map);
        }

        private void TrySpawnWall(IntVec3 c, Map map, ResolveParams rp, ThingDef wallStuff)
        {
            if (!c.InBounds(map))
                return;

            if (TryClearCell(c, map))
            {
                if (rp.chanceToSkipWallBlock != null && Rand.Chance(rp.chanceToSkipWallBlock.Value))
                {
                    return;
                }
                Thing thing = ThingMaker.MakeThing(RimWorld.ThingDefOf.Wall, wallStuff);
                thing.SetFaction(rp.faction, null);
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

        private void TrySpawnRoof(IntVec3 c, Map map)
        {
            if (map.roofGrid.RoofAt(c) == null)
                map.roofGrid.SetRoof(c, RoofDefOf.RoofConstructed);
        }

    }

}
