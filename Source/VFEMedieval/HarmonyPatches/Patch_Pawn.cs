using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

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
