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

    public class SymbolResolver_MedievalEdgeDefense : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            // Make sandbag placements
            var sandbagParams = rp;
            sandbagParams.GetCustom<MedievalResolveParams>(MedievalResolveParams.Name).symmetricalSandbags = true;
            BaseGen.symbolStack.Push("castleEdgeSandbags", sandbagParams);

            // Generate perimeter
            var edgeWallParams = rp;
            edgeWallParams.GetCustom<MedievalResolveParams>(MedievalResolveParams.Name).edgeWallDef = ThingDefOf.CobblestoneWall;
            edgeWallParams.wallStuff = Find.World.NaturalRockTypesIn(BaseGen.globalSettings.map.Tile).Select(t => t.building.mineableThing).Where(t => t.IsStuff).
                RandomElementByWeight(t => (1 + t.stuffProps.statOffsets.GetStatOffsetFromList(StatDefOf.MaxHitPoints)) * t.stuffProps.statFactors.GetStatFactorFromList(StatDefOf.MaxHitPoints) * t.stuffProps.commonality);
            BaseGen.symbolStack.Push("castleEdgeWalls", rp);
        }

    }

}
