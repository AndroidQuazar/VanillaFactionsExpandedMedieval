using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFEMedieval
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {

        static HarmonyPatches()
        {
            //HarmonyInstance.DEBUG = true;
            VFEMedieval.harmonyInstance.PatchAll();

            // Compatibility with RimCuisine 2
            if (ModCompatibilityCheck.RimCuisine2BottlingAndBooze)
            {
                var harmonyPatches = GenTypes.GetTypeInAnyAssembly("RimCuisineBBDrugPolicies.HarmonyPatches", "RimCuisineBBDrugPolicies");
                if (harmonyPatches != null)
                    VFEMedieval.harmonyInstance.Patch(AccessTools.Method(harmonyPatches, "GenerateStartingDrugPolicies_PostFix"), transpiler: new HarmonyMethod(typeof(Patch_RimCuisineBBDrugPolicies_HarmonyPatches.manual_GenerateStartingDrugPolicies_PostFix), "Transpiler"));
                else
                    Log.Error("Could not find type RimCuisineBBDrugPolicies.HarmonyPatches in RimCuisine 2: Bottling and Booze Expansion");
            }
        }

    }

}
