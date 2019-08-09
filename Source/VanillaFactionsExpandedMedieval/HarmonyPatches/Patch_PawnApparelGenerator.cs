﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFEMedieval
{

    public static class Patch_PawnApparelGenerator
    {

        public static class PossibleApparelSet
        {

            public static class manual_CoatButNoShirt
            {

                public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    var instructionList = instructions.ToList();

                    var apparelLayerDefOfShellInfo = AccessTools.Field(typeof(RimWorld.ApparelLayerDefOf), nameof(RimWorld.ApparelLayerDefOf.Shell));
                    var apparelLayerDefOfOuterShellInfo = AccessTools.Field(typeof(ApparelLayerDefOf), nameof(ApparelLayerDefOf.OuterShell));

                    for (int i = 0; i < instructionList.Count; i++)
                    {
                        var instruction = instructionList[i];

                        // Also have the generator consider OuterShell as an appropriate clothing layer
                        if (instruction.opcode == OpCodes.Beq)
                        {
                            var prevInstruction = instructionList[i - 1];
                            if (prevInstruction.opcode == OpCodes.Ldsfld && instruction.operand == apparelLayerDefOfShellInfo)
                            {
                                yield return instruction;
                                yield return instructionList[i - 2]; // apparelLayerDef
                                yield return new CodeInstruction(OpCodes.Ldsfld, apparelLayerDefOfOuterShellInfo); // ApparelLayerDefOf.OuterShell
                                instruction = instruction.Clone(); //  if (... || apparelLayerDef == ApparelLayerDefOf.OuterShell || ...)
                            }
                        }

                        yield return instruction;
                    }
                }

            }

        }

        [HarmonyPatch(typeof(PawnApparelGenerator), nameof(PawnApparelGenerator.GenerateStartingApparelFor))]
        public static class GenerateStartingApparelFor
        {

            public static void Postfix(Pawn pawn)
            {
                // Change the colour of appropriate apparel items to match the pawn's faction's colour
                if (pawn.apparel != null && pawn.Faction != null)
                {
                    foreach (var apparel in pawn.apparel.WornApparel)
                    {
                        var thingDefExtension = apparel.def.GetModExtension<ThingDefExtension>() ?? ThingDefExtension.defaultValues;
                        if (thingDefExtension.useFactionColour && (thingDefExtension.overrideKindDefApparelColour || pawn.kindDef.apparelColor == Color.white))
                            apparel.SetColor(pawn.Faction.Color);
                    }
                }
            }

        }

    }

}
