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
                        policy[ThingDefOf.VFEM_Wine].allowedForJoy = true;
                    }
                }
            }

        }

    }

}
