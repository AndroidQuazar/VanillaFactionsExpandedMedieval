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

    public class SymbolResolver_MedievalSettlement : SymbolResolver
    {

        public override void Resolve(ResolveParams rp)
        {
            rp.SetCustom(VFEResolveParams.Name, new VFEResolveParams(), true);
            var medievalResolveParams = rp.GetCustom<VFEResolveParams>(VFEResolveParams.Name);

            var faction = rp.faction ?? Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
            float num2 = rp.rect.Area / 144f * 0.17f;
            BaseGen.globalSettings.minEmptyNodes = (num2 >= 1f) ? GenMath.RoundRandom(num2) : 0;

            // Generate pawns
            if (medievalResolveParams.generatePawns != false)
            {
                var map = BaseGen.globalSettings.map;
                var pawnParams = rp;
                pawnParams.rect = rp.rect;
                pawnParams.faction = faction;
                pawnParams.singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(faction, new LordJob_DefendBase(faction, rp.rect.CenterCell), map, null);
                pawnParams.pawnGroupKindDef = rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Settlement;
                pawnParams.singlePawnSpawnCellExtraPredicate = rp.singlePawnSpawnCellExtraPredicate ?? ((IntVec3 x) => map.reachability.CanReachMapEdge(x, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)));
                if (pawnParams.pawnGroupMakerParams == null)
                {
                    pawnParams.pawnGroupMakerParams = new PawnGroupMakerParms();
                    pawnParams.pawnGroupMakerParams.tile = map.Tile;
                    pawnParams.pawnGroupMakerParams.faction = faction;
                    var pawnGroupMakerParams = pawnParams.pawnGroupMakerParams;
                    float? settlementPawnGroupPoints = rp.settlementPawnGroupPoints;
                    pawnGroupMakerParams.points = (settlementPawnGroupPoints == null) ? DefaultPawnsPoints.RandomInRange : settlementPawnGroupPoints.Value;
                    pawnParams.pawnGroupMakerParams.inhabitants = true;
                    pawnParams.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
                }
                BaseGen.symbolStack.Push("pawnGroup", pawnParams);
            }

            // Generate outdoor lighting
            if (medievalResolveParams.outdoorLighting != false)
                BaseGen.symbolStack.Push("outdoorLighting", rp);

            // Generate firefoam poppers if applicable
            if (faction.def.techLevel >= TechLevel.Industrial)
            {
                int num3 = (!Rand.Chance(0.75f)) ? 0 : GenMath.RoundRandom((float)rp.rect.Area / 400f);
                for (int i = 0; i < num3; i++)
                {
                    ResolveParams firefoamParams = rp;
                    firefoamParams.faction = faction;
                    BaseGen.symbolStack.Push("firefoamPopper", firefoamParams);
                }
            }

            // Generate defences
            float towerRadius = 0;
            float? curTowerRadius = medievalResolveParams.towerRadius;
            if (curTowerRadius != null)
                towerRadius = rp.edgeDefenseWidth.Value;
            else if (rp.rect.Width >= 20 && rp.rect.Height >= 20)
                towerRadius = (Rand.Bool) ? 3.9f : 4.9f;
            if (towerRadius > 0)
            {
                ResolveParams defenceParams = rp;
                var medievalDefenceParams = defenceParams.GetCustom<VFEResolveParams>(VFEResolveParams.Name);
                defenceParams.faction = faction;
                medievalDefenceParams.towerRadius = towerRadius;
                BaseGen.symbolStack.Push("VFEM_medievalEdgeDefense", defenceParams);
            }

            // Map edge reachability
            int towerRadCeil = Mathf.CeilToInt(towerRadius);
            ResolveParams edgeReachParams = rp;
            edgeReachParams.rect = rp.rect.ContractedBy(towerRadCeil);
            edgeReachParams.faction = faction;
            BaseGen.symbolStack.Push("ensureCanReachMapEdge", edgeReachParams);

            // Generate base
            ResolveParams baseParams = rp;
            baseParams.rect = rp.rect.ContractedBy(towerRadCeil);
            baseParams.faction = faction;
            BaseGen.symbolStack.Push("basePart_outdoors", baseParams);

            // Generate flooring
            ResolveParams floorParams = rp;
            floorParams.floorDef = TerrainDefOf.Bridge;
            bool? floorOnlyIfTerrainSupports = rp.floorOnlyIfTerrainSupports;
            floorParams.floorOnlyIfTerrainSupports = floorOnlyIfTerrainSupports == null || floorOnlyIfTerrainSupports.Value;
            BaseGen.symbolStack.Push("floor", floorParams);
        }

        public static readonly FloatRange DefaultPawnsPoints = new FloatRange(1150f, 1600f);

    }

}
