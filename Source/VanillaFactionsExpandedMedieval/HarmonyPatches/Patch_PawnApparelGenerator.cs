using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VanillaFactionsExpandedMedieval
{

    public static class Patch_PawnApparelGenerator
    {

        [HarmonyPatch(typeof(PawnApparelGenerator), nameof(PawnApparelGenerator.GenerateStartingApparelFor))]
        public static class GenerateStartingApparelFor
        {

            public static void Postfix(Pawn pawn)
            {
                // Change the colour of appropriate apparel items to match the pawn's faction's colour
                if (pawn.Faction != null)
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
