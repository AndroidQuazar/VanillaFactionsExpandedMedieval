using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFEMedieval
{

    public static class Patch_StatWorker
    {

        [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.ShouldShowFor))]
        public static class ShouldShowFor
        {

            public static void Postfix(StatWorker __instance, StatDef ___stat, StatRequest req, ref bool __result)
            {
                // Stats with Apparel category can also show on shields
                if (!__result && !___stat.alwaysHide && req.Def is ThingDef tDef && (___stat.showIfUndefined || tDef.statBases.StatListContains(___stat))  && ___stat.category == StatCategoryDefOf.Apparel)
                    __result = tDef.IsShield();
            }

        }

    }

}
