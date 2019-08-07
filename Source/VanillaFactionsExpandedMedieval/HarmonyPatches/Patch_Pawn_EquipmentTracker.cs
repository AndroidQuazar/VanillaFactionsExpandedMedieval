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

    public static class Patch_Pawn_EquipmentTracker
    {

        [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.AddEquipment))]
        public static class AddEquipment
        {

            public static bool Prefix(Pawn_EquipmentTracker __instance, ThingWithComps newEq)
            {
                if (__instance.Primary != null && __instance.Primary.def.IsShield() && __instance.EquippedShieldCount() < 2)
                {
                    var equipmentOwner = (ThingOwner<ThingWithComps>)NonPublicFields.Pawn_EquipmentTracker_equipment.GetValue(__instance);
                    equipmentOwner.TryAdd(newEq);
                    equipmentOwner.InnerListForReading.SortBy(t => t.def.IsShield());
                    return false;
                }
                return true;
            }

        }

        [HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.MakeRoomFor))]
        public static class MakeRoomFor
        {

            public static bool Prefix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
            {
                return __instance.Primary == null || !__instance.Primary.def.IsShield() || __instance.EquippedShieldCount() >= 2;
            }

        }

        //[HarmonyPatch(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.TryDropEquipment))]
        //public static class TryDropEquipment
        //{

        //    public static void Postfix(ThingWithComps resultingEq, bool __result)
        //    {
        //        // If a shield was dropped from the equipment tracker, set equippedOffhand to false
        //        if (__result && resultingEq != null && resultingEq.TryGetComp<CompShield>() is CompShield shieldComp)
        //            shieldComp.equippedOffhand = false;
        //    }

        //}

    }

}
