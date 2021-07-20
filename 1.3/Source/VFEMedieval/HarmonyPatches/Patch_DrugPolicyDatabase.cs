using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VFEMedieval
{
    public static class Patch_DrugPolicyDatabase
    {
        [HarmonyPatch(typeof(DrugPolicyDatabase), "GenerateStartingDrugPolicies")]
        public static class GenerateStartingDrugPolicies
        {
            public static void Postfix(ref List<DrugPolicy> ___policies)
            {
                // Count wine as a recreational drug
                for (int i = 0; i < ___policies.Count; i++)
                {
                    var policy = ___policies[i];
                    if (policy.label == "SocialDrugs".Translate() || policy.label == "OneDrinkPerDay".Translate())
                    {
                        policy[VFEM_DefOf.VFEM_Wine].allowedForJoy = true;
                    }
                }
            }
        }
    }
}