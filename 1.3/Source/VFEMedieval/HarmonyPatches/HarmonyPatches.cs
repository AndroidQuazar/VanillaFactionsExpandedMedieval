using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

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

    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "CanInteractNowWith")]
    public static class CanInteractNowWith_Patch
    {
        public static void Postfix(ref bool __result, Pawn ___pawn, Pawn recipient)
        {
            if (___pawn.WearsApparel(VFEM_DefOf.VFEM_Apparel_PlagueMask) || recipient.WearsApparel(VFEM_DefOf.VFEM_Apparel_PlagueMask))
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipment")]
    public static class DrawEquipment_Patch
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn ___pawn, Vector3 rootLoc, Rot4 pawnRotation, PawnRenderFlags flags)
        {
            if (___pawn.jobs?.curDriver is JobDriver_PlayArchery playArchery)
            {
                playArchery.DrawEquipment(rootLoc, pawnRotation, flags);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SettlementUtility), "AffectRelationsOnAttacked")]
    internal static class Patch_AffectRelationsOnAttacked
    {
        private static bool Prefix(MapParent mapParent, ref TaggedString letterText)
        {
            if (mapParent is Site site && site.parts != null)
            {
                foreach (var part in site.parts)
                {
                    if (part.def == VFEM_DefOf.VFE_Skirmish)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}