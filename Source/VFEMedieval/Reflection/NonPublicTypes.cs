using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFEMedieval
{

    public static class NonPublicTypes
    {

        [StaticConstructorOnStartup]
        public static class RimCuisine2
        {

            static RimCuisine2()
            {
                if (ModCompatibilityCheck.RimCuisine2BottlingAndBooze)
                {
                    RCBBDefOf = GenTypes.GetTypeInAnyAssembly("RimCuisineBBDrugPolicies.RCBBDefOf", "RimCuisineBBDrugPolicies");
                }
            }

            public static Type RCBBDefOf;

        }

    }

}
