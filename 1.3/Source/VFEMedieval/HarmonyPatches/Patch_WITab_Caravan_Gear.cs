using HarmonyLib;
using RimWorld.Planet;
using Verse;

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
                if (t == VFEM_DefOf.VFEM_Wine)
                    __result = false;
            }
        }
    }
}