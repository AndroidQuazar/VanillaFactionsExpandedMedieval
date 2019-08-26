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

namespace VFEMedieval
{

    public static class Patch_PawnGraphicSet
    {

        [HarmonyPatch(typeof(PawnGraphicSet), nameof(PawnGraphicSet.MatsBodyBaseAt))]
        public static class MatsBodyBaseAt
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return Patch_PawnRenderer.RenderPawnInternal.Transpiler(instructions);
            }

        }

        [HarmonyPatch(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics))]
        public static class ResolveAllGraphics
        {

            public static void Postfix(PawnGraphicSet __instance)
            {
                // If the pawn's a pack animal and is part of a medieval faction, use medieval pack texture if applicable
                if (__instance.pawn.Faction != null && __instance.pawn.Faction.def.techLevel == TechLevel.Medieval && __instance.pawn.RaceProps.packAnimal)
                {
                    var medievalPackTexture = ContentFinder<Texture2D>.Get(__instance.nakedGraphic.path + "MedievalPack_south", false);
                    if (medievalPackTexture != null)
                        __instance.packGraphic = GraphicDatabase.Get<Graphic_Multi>(__instance.nakedGraphic.path + "MedievalPack", ShaderDatabase.CutoutComplex, __instance.nakedGraphic.drawSize, __instance.pawn.Faction.Color);
                }
            }

        }

    }

}
