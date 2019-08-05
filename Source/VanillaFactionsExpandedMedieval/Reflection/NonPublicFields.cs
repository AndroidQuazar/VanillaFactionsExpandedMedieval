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
    public static class NonPublicFields
    {

        public static FieldInfo Pawn_EquipmentTracker_equipment = typeof(Pawn_EquipmentTracker).GetField("equipment", BindingFlags.NonPublic | BindingFlags.Instance);

    }

}
