using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFEMedieval
{

    public static class Patch_RimCuisineBBDrugPolicies_HarmonyPatches
    {

        public static class manual_GenerateStartingDrugPolicies_PostFix
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var RC2_WineInfo = AccessTools.Field(NonPublicTypes.RimCuisine2.RCBBDefOf, "RC2_Wine");

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Prevent RimCuisine2's patch from registering its version of wine as a recreational drug
                    if (instruction.opcode == OpCodes.Stfld)
                    {
                        var twoInstructionsAhead = instructionList[i + 2];
                        if (twoInstructionsAhead.opcode == OpCodes.Ldsfld && twoInstructionsAhead.operand == RC2_WineInfo)
                        {
                            instructionList.RemoveAt(i + 1);
                            while (instructionList[i + 1].opcode != OpCodes.Ldloc_1)
                                instructionList.RemoveAt(i + 1);
                        }
                    }

                    yield return instruction;
                }
            }

        }

    }

}
