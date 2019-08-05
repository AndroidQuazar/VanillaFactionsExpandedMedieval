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

    public static class Patch_ThingDef
    {

        [HarmonyPatch(typeof(ThingDef), nameof(ThingDef.IsApparel), MethodType.Getter)]
        public static class get_IsApparel
        {

            public static void Postfix(ThingDef __instance, ref bool __result)
            {
                // Don't count shields as apparel
                if (__result && __instance.IsShield())
                    __result = false;
            }

        }

    }

}
