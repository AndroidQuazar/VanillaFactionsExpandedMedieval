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
    public static class ModCompatibilityCheck
    {

        public static bool DualWield = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Dual Wield");

    }

}
