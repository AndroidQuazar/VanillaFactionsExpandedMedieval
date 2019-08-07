using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VanillaFactionsExpandedMedieval
{

    public static class Patch_PawnRenderer
    {

        [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) })]
        public static class RenderPawnInternal
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var apparelLayerDefOfShellInfo = AccessTools.Field(typeof(RimWorld.ApparelLayerDefOf), nameof(RimWorld.ApparelLayerDefOf.Shell));
                var topApparelLayerDefInfo = AccessTools.Method(typeof(RenderPawnInternal), nameof(TopApparelLayerDef));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Replace references to Shell layer with references to our OuterShell layer
                    if (instruction.opcode == OpCodes.Ldsfld && instruction.operand == apparelLayerDefOfShellInfo)
                    {
                        yield return instruction; // ApparelLayerDefOf.Shell
                        instruction = new CodeInstruction(OpCodes.Call, topApparelLayerDefInfo); // TopApparelLayerDef(ApparelLayerDefOf.Shell)
                    }

                    yield return instruction;
                }
            }

            private static ApparelLayerDef TopApparelLayerDef(ApparelLayerDef original)
            {
                return ApparelLayerDefOf.OuterShell;
            }

        }

    }

}
