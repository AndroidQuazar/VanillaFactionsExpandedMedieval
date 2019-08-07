using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaFactionsExpandedMedieval
{

    [StaticConstructorOnStartup]
    public static class NonPublicMethods
    {

        static NonPublicMethods()
        {
            if (ModCompatibilityCheck.DualWield)
            {
                DualWield_Ext_Pawn_EquipmentTracker_TryGetOffHandEquipment = (FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool>)
                    Delegate.CreateDelegate(typeof(FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool>),
                    GenTypes.GetTypeInAnyAssemblyNew("DualWield.Ext_Pawn_EquipmentTracker", "DualWield").GetMethod("TryGetOffHandEquipment", BindingFlags.Public | BindingFlags.Static));
            }
        }

        public static ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool> ArmorUtility_ApplyArmor = (ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool>)
            Delegate.CreateDelegate(typeof(ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool>), typeof(ArmorUtility).GetMethod("ApplyArmor", BindingFlags.NonPublic | BindingFlags.Static));

        public static FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool> DualWield_Ext_Pawn_EquipmentTracker_TryGetOffHandEquipment;

        public delegate void ApplyArmourDelegate<A, B, C, D, E, F, G>(ref A first, B second, C third, D fourth, ref E fifth, F sixth, out G seventh);
        public delegate C FuncOut<A, B, C>(A first, out B second);

    }

}
