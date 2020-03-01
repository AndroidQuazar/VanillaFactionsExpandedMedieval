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

namespace VFEMedieval
{

    public class SymbolResolver_BasePart_Indoors_Leaf_Winery : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp) &&
                BaseGen.globalSettings.basePart_barracksResolved >= BaseGen.globalSettings.minBarracks && 
                BaseGen.globalSettings.basePart_breweriesCoverage + (float)rp.rect.Area / BaseGen.globalSettings.mainRect.Area < MaxCoverage && 
                (rp.faction == null || rp.faction.def.techLevel >= TechLevel.Medieval);
        }

        public override void Resolve(ResolveParams rp)
        {
            BaseGen.symbolStack.Push("VFEM_winery", rp);
            BaseGen.globalSettings.basePart_breweriesCoverage += (float)rp.rect.Area / BaseGen.globalSettings.mainRect.Area;
        }

        private const float MaxCoverage = 0.08f;

    }

}
