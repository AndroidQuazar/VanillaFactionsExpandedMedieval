using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEMedieval
{

    [StaticConstructorOnStartup]
    public static class ModCompatibilityCheck
    {

        public static bool RimCuisine2BottlingAndBooze = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "[1.0] RimCuisine 2: Bottling and Booze Expansion");

        public static bool VanillaFurnitureExpandedSecurity = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Vanilla Furniture Expanded - Security");

    }

}
