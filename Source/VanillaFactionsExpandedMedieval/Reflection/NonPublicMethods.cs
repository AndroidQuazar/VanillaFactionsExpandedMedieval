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

        public static ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool> ArmorUtility_ApplyArmor = (ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool>)
            Delegate.CreateDelegate(typeof(ApplyArmourDelegate<float, float, float, Thing, DamageDef, Pawn, bool>), typeof(ArmorUtility).GetMethod("ApplyArmor", BindingFlags.NonPublic | BindingFlags.Static));

        public delegate void ApplyArmourDelegate<A, B, C, D, E, F, G>(ref A first, B second, C third, D fourth, ref E fifth, F sixth, out G seventh);

    }

}
