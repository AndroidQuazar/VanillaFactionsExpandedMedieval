using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFEMedieval
{

    [StaticConstructorOnStartup]
    public static class NonPublicFields
    {

        public static FieldInfo DamageInfo_armorPenetrationInt = AccessTools.Field(typeof(DamageInfo), "armorPenetrationInt");

    }

}
