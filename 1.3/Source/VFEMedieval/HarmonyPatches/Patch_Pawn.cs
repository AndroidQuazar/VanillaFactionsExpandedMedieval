using HarmonyLib;
using Verse;

namespace VFEMedieval
{
    public static class Patch_Pawn
    {
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
        public static class PreApplyDamage
        {
            public static void Prefix(Pawn __instance, ref DamageInfo dinfo)
            {
                // I tried to do this via comp but couldn't get it to work properly
                BlackKnightUtility.ModifyDamageInfo(__instance, ref dinfo);
            }
        }
    }
}