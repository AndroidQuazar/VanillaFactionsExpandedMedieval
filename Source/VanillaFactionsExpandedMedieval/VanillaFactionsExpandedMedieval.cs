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

    public class VanillaFactionsExpandedMedieval : Mod
    {

        public VanillaFactionsExpandedMedieval(ModContentPack content) : base(content)
        {
            HarmonyInstance = HarmonyInstance.Create("OskarPotocki.VanillaFactionsExpandedMedieval");
        }

        public static HarmonyInstance HarmonyInstance;

    }

}
