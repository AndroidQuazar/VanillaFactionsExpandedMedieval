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
            BaseGen.symbolStack.Push("castleEdgeWalls", rp);
        }

    }

}
