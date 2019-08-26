using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFEMedieval
{

    public class VanillaFactionsExpanded : Mod
    {

        public VanillaFactionsExpanded(ModContentPack content) : base(content)
        {
            harmonyInstance = HarmonyInstance.Create("OskarPotocki.VanillaFactionsExpanded");
        }

        public static HarmonyInstance harmonyInstance;

    }

}
