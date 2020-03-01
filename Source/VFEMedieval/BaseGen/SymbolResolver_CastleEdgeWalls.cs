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

    [StaticConstructorOnStartup]
    public class SymbolResolver_CastleEdgeWalls : SymbolResolver
    {

        private static List<IntVec3> potentialTowerDoorCells = new List<IntVec3>();

        public override void Resolve(ResolveParams rp)
        {
            var perimeterWallCells = rp.rect.EdgeCells.ToList();
            var map = BaseGen.globalSettings.map;
            var faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction();
            var medievalrp = rp.GetCustom<VFEResolveParams>(VFEResolveParams.Name);

            // Generate towers on each corner of the rect
            var corners = rp.rect.Corners.ToList();
            float towerRadius = medievalrp?.towerRadius ?? 3.9f;
            var wallDef = medievalrp?.edgeWallDef ?? RimWorld.ThingDefOf.Wall;
            var wallStuff = rp.wallStuff ?? GenStuff.RandomStuffInexpensiveFor(wallDef, faction);
            for (int i = 0; i < corners.Count; i++)
            {
                var corner = corners[i];
                var towerCells = GenRadial.RadialCellsAround(corner, towerRadius, true).ToList();
                if (ValidAreaForTower(towerCells, map))
                {
                    perimeterWallCells = perimeterWallCells.Where(c => !towerCells.Contains(c)).ToList();
                    var towerInteriorCells = GenRadial.RadialCellsAround(corner, towerRadius - 1.42f, true).ToList();
                    var towerExteriorCells = towerCells.Where(c => !towerInteriorCells.Contains(c)).ToList();
                    TryGenerateTower(towerExteriorCells, towerInteriorCells, map, rp, wallDef, wallStuff);
                    potentialTowerDoorCells = potentialTowerDoorCells.Concat(towerExteriorCells.Where(c => rp.rect.ContractedBy(1).Contains(c) && ValidDoorCell(c, map))).ToList();
                }
            }

            // Generate tower entrances
            if (medievalrp.hasDoors != false)
            {
                for (int i = 0; i < potentialTowerDoorCells.Count; i++)
                {
                    var pos = potentialTowerDoorCells[i];
                    TrySpawnFloor(pos, map);
                    var doorStuff = faction.def.techLevel.IsNeolithicOrWorse() ? RimWorld.ThingDefOf.WoodLog : RimWorld.ThingDefOf.Steel;
                    var door = ThingMaker.MakeThing(RimWorld.ThingDefOf.Door, doorStuff);
                    door.SetFaction(faction);
                    GenSpawn.Spawn(door, pos, map);
                }
            }
            potentialTowerDoorCells.Clear();

            // Generate perimeter walls
            for (int i = 0; i < perimeterWallCells.Count; i++)
            {
                var pos = perimeterWallCells[i];
                TrySpawnWall(pos, map, rp, wallDef, wallStuff);
            }
        }

        private bool ValidAreaForTower(List<IntVec3> cells, Map map)
        {
            return !cells.Any(c => c.GetThingList(map).Any(t => t.def.building != null && t.def.building.isNaturalRock));
        }

        private bool ValidDoorCell(IntVec3 c, Map map)
        {
            return (!(c + IntVec3.North).Impassable(map) && !(c + IntVec3.South).Impassable(map)) || (!(c + IntVec3.West).Impassable(map) && !(c + IntVec3.East).Impassable(map));
        }

        private void TryGenerateTower(List<IntVec3> exteriorCells, List<IntVec3> interiorCells, Map map, ResolveParams rp, ThingDef wallDef, ThingDef wallStuff)
        {
            // Walls
            for (int i = 0; i < exteriorCells.Count; i++)
            {
                var pos = exteriorCells[i];
                TrySpawnWall(pos, map, rp, wallDef, wallStuff);
            }

            // Interior
            for (int i = 0; i < interiorCells.Count; i++)
            {
                var pos = interiorCells[i];
                TryDoTowerInterior(pos, map, rp);
            }
        }

        private void TryDoTowerInterior(IntVec3 c, Map map, ResolveParams rp)
        {
            // Not in bounds
            if (!c.InBounds(map))
                return;

            // Interior
            TrySpawnFloor(c, map);

            // Roof
            if (rp.noRoof != true)
                TrySpawnRoof(c, map);
        }

        private void TrySpawnWall(IntVec3 c, Map map, ResolveParams rp, ThingDef wallDef, ThingDef wallStuff)
        {
            // Not in bounds
            if (!c.InBounds(map))
                return;

            if (TryClearCell(c, map))
            {
                // Try and make it buildable
                if (!GenConstruct.CanBuildOnTerrain(wallDef, c, map, Rot4.North))
                {
                    if (GenConstruct.CanBuildOnTerrain(TerrainDefOf.PavedTile, c, map, Rot4.North))
                        map.terrainGrid.SetTerrain(c, TerrainDefOf.PavedTile);
                    else if (GenConstruct.CanBuildOnTerrain(TerrainDefOf.Bridge, c, map, Rot4.North))
                        map.terrainGrid.SetTerrain(c, TerrainDefOf.Bridge);
                }
 
                if (!GenConstruct.CanBuildOnTerrain(wallDef, c, map, Rot4.North) || (rp.chanceToSkipWallBlock != null && Rand.Chance(rp.chanceToSkipWallBlock.Value)))
                    return;

                var wall = ThingMaker.MakeThing(wallDef, wallStuff);
                wall.SetFaction(rp.faction, null);
                GenSpawn.Spawn(wall, c, map, WipeMode.Vanish);
            }
        }

        private bool TryClearCell(IntVec3 c, Map map)
        {
            var thingList = c.GetThingList(map);
            for (int i = 0; i < thingList.Count; i++)
            {
                var thing = thingList[i];
                if (!thing.def.destroyable || thing.def == SymbolResolver_CastleEdgeSandbags.DefToUse || (thing.def.building != null && thing.def.building.isNaturalRock))
                    return false;
            }

            for (int i = 0; i < thingList.Count; i++)
                thingList[i].Destroy();
            return true;
        }

        private void TrySpawnFloor(IntVec3 c, Map map)
        {
            if (!c.InBounds(map))
                return;

            if (TryClearCell(c, map))
            {
                var floorDef = GenConstruct.CanBuildOnTerrain(TerrainDefOf.WoodPlankFloor, c, map, Rot4.North) ? TerrainDefOf.WoodPlankFloor : TerrainDefOf.Bridge;
                map.terrainGrid.SetTerrain(c, floorDef);
            }
        }

        private void TrySpawnRoof(IntVec3 c, Map map)
        {
            if (map.roofGrid.RoofAt(c) == null)
                map.roofGrid.SetRoof(c, RoofDefOf.RoofConstructed);
        }

    }

}
