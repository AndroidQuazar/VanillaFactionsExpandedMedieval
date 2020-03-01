using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;

namespace VFEMedieval
{

    public static class Patch_WITab_Caravan_Gear
    {

        [HarmonyPatch(typeof(WITab_Caravan_Gear), "IsVisibleWeapon")]
        public static class IsVisibleWeapon
        {

            public static void Postfix(ThingDef t, ref bool __result)
            {
                // Don't show wine in the tab (just like wood logs and beer)
                if (t == ThingDefOf.VFEM_Wine)
                    __result = false;
            }

        }

    }

}
