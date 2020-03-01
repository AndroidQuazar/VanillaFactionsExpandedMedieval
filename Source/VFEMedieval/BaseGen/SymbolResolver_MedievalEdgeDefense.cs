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

    public class SymbolResolver_MedievalEdgeDefense : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            var edgeWallParams = rp;
            edgeWallParams.wallStuff = Find.World.NaturalRockTypesIn(BaseGen.globalSettings.map.Tile).Select(t => t.building.mineableThing).Where(t => t.IsStuff).
                RandomElementByWeight(t => (1 + t.stuffProps.statOffsets.GetStatOffsetFromList(StatDefOf.MaxHitPoints)) * t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MaxHitPoints) * t.stuffProps.commonality);

            // Make sandbag placements
            var sandbagParams = rp;
            sandbagParams.wallStuff = edgeWallParams.wallStuff?.butcherProducts?.FirstOrDefault(p => p.thingDef.IsStuff)?.thingDef;
            sandbagParams.GetCustom<VFEResolveParams>(VFEResolveParams.Name).symmetricalSandbags = true;
            BaseGen.symbolStack.Push("VFEM_castleEdgeSandbags", sandbagParams);

            // Generate perimeter
            edgeWallParams.GetCustom<VFEResolveParams>(VFEResolveParams.Name).edgeWallDef = ThingDefOf.VFEM_CobblestoneWall;
            BaseGen.symbolStack.Push("VFEM_castleEdgeWalls", rp);
        }

    }

}
