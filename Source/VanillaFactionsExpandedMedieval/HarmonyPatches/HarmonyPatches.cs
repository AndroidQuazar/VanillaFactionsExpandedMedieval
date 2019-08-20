using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFEMedieval
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {

        static HarmonyPatches()
        {
            //HarmonyInstance.DEBUG = true;
            VFEMedieval.HarmonyInstance.PatchAll();

            // PawnApparelGenerator.PossibleApparelSet.CoatButNoShirt
            VFEMedieval.HarmonyInstance.Patch(typeof(PawnApparelGenerator).GetNestedType("PossibleApparelSet", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod("CoatButNoShirt", BindingFlags.Public | BindingFlags.Instance),
                transpiler: new HarmonyMethod(typeof(Patch_PawnApparelGenerator.PossibleApparelSet.manual_CoatButNoShirt), "Transpiler"));

            // Dual Wield
            if (ModCompatibilityCheck.DualWield)
            {
                var addHumanlikeOrdersPatch = GenTypes.GetTypeInAnyAssemblyNew("DualWield.Harmony.FloatMenuMakerMap_AddHumanlikeOrders", "DualWield.Harmony");
                if (addHumanlikeOrdersPatch != null)
                    VFEMedieval.HarmonyInstance.Patch(AccessTools.Method(addHumanlikeOrdersPatch, "Postfix"),
                        transpiler: new HarmonyMethod(typeof(Patch_DualWield_Harmony_FloatMenuMakerMap_AddHumanlikeOrders.manual_Postfix), "Transpiler"));
                else
                    Log.Error("Could not find type DualWield.Harmony.FloatMenuMakerMap_AddHumanlikeOrders in Dual Wield");

                var extEquipmentTracker = GenTypes.GetTypeInAnyAssemblyNew("DualWield.Ext_Pawn_EquipmentTracker", "DualWield");
                if (extEquipmentTracker != null)
                    VFEMedieval.HarmonyInstance.Patch(AccessTools.Method(extEquipmentTracker, "MakeRoomForOffHand"),
                        postfix: new HarmonyMethod(typeof(Patch_DualWield_Ext_Pawn_EquipmentTracker.manual_MakeRoomForOffHand), "Postfix"));
                else
                    Log.Error("Could not find type DualWield.Ext_Pawn_EquipmentTracker in Dual Wield");
            }

            // Facial Stuff
            if (ModCompatibilityCheck.FacialStuff)
            {
                var humanBipedDrawer = GenTypes.GetTypeInAnyAssemblyNew("FacialStuff.HumanBipedDrawer", "FacialStuff");
                if (humanBipedDrawer != null)
                    VFEMedieval.HarmonyInstance.Patch(AccessTools.Method(humanBipedDrawer, "DrawApparel"), transpiler: new HarmonyMethod(typeof(Patch_PawnRenderer.RenderPawnInternal), "Transpiler"));
                else
                    Log.Error("Could not find type FacialStuff.HumanBipedDrawer in Facial Stuff");
            }

            // RPG Style Inventory
            if (ModCompatibilityCheck.RPGStyleInventory)
            {
                var detailedRPGGearTab = GenTypes.GetTypeInAnyAssemblyNew("Sandy_Detailed_RPG_Inventory.Sandy_Detailed_RPG_GearTab", "Sandy_Detailed_RPG_Inventory");
                if (detailedRPGGearTab != null)
                {
                    Patch_Sandy_Detailed_RPG_GearTab_Sandy_Detailed_RPG_Inventory.manual_TryDrawOverallArmor.DetailedRPGGearTab = detailedRPGGearTab;
                    VFEMedieval.HarmonyInstance.Patch(AccessTools.Method(detailedRPGGearTab, "TryDrawOverallArmor"),
                        transpiler: new HarmonyMethod(typeof(Patch_Sandy_Detailed_RPG_GearTab_Sandy_Detailed_RPG_Inventory.manual_TryDrawOverallArmor), "Transpiler"));
                    VFEMedieval.HarmonyInstance.Patch(AccessTools.Method(detailedRPGGearTab, "TryDrawOverallArmor1"),
                        transpiler: new HarmonyMethod(typeof(Patch_Sandy_Detailed_RPG_GearTab_Sandy_Detailed_RPG_Inventory.manual_TryDrawOverallArmor1), "Transpiler"));
                }
                    
                else
                    Log.Error("Could not find type Sandy_Detailed_RPG_Inventory.Sandy_Detailed_RPG_GearTab in RPG Style Inventory");
            }
        }

    }

}
