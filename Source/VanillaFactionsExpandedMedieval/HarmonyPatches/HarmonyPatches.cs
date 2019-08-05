using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VanillaFactionsExpandedMedieval
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {

        static HarmonyPatches()
        {
            //HarmonyInstance.DEBUG = true;
            VanillaFactionsExpandedMedieval.HarmonyInstance.PatchAll();

            // Dual Wield
            if (ModCompatibilityCheck.DualWield)
            {
                var addHumanlikeOrdersPatch = GenTypes.GetTypeInAnyAssemblyNew("DualWield.Harmony.FloatMenuMakerMap_AddHumanlikeOrders", "DualWield.Harmony");
                if (addHumanlikeOrdersPatch != null)
                    VanillaFactionsExpandedMedieval.HarmonyInstance.Patch(AccessTools.Method(addHumanlikeOrdersPatch, "Postfix"),
                        transpiler: new HarmonyMethod(typeof(Patch_DualWield_Harmony_FloatMenuMakerMap_AddHumanlikeOrders.manual_Postfix), "Transpiler"));
                else
                    Log.Error("Could not find type DualWield.Harmony.FloatMenuMakerMap_AddHumanlikeOrders in Dual Wield");
            }
        }

    }

}
