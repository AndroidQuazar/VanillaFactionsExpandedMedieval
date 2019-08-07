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

    public static class Patch_CompEquippable
    {

        [HarmonyPatch(typeof(CompEquippable), nameof(CompEquippable.GetVerbsCommands))]
        public static class GetVerbsCommands
        {

            public static void Postfix(CompEquippable __instance, ref IEnumerable<Command> __result)
            {
                // Clear gizmos if parent is a shield and isn't usable
                if (__instance.parent.IsShield(out CompShield shieldComp) && !shieldComp.UsableNow)
                    __result = __result.Where(g => false);
            }

        }

    }

}
