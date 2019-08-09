﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFEMedieval
{

    public static class Patch_GenStep_Settlement
    {

        [HarmonyPatch(typeof(GenStep_Settlement), "ScatterAt")]
        public static class ScatterAt
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var settlementGenerationSymbolInfo = AccessTools.Method(typeof(ScatterAt), nameof(SettlementGenerationSymbol));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    if (instruction.opcode == OpCodes.Ldstr && (string)instruction.operand == "settlement")
                    {
                        yield return instruction; // "settlement"
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 5); // faction
                        instruction = new CodeInstruction(OpCodes.Call, settlementGenerationSymbolInfo); // SettlementGenerationSymbol("settlement", faction)
                    }

                    yield return instruction;
                }
            }

            private static string SettlementGenerationSymbol(string original, Faction faction)
            {
                var factionDefExtension = faction.def.GetModExtension<FactionDefExtension>() ?? FactionDefExtension.defaultValues;
                return factionDefExtension.settlementGenerationSymbol ?? original;
            }

        }

    }

}
