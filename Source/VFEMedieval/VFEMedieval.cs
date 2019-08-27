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

    public class VFEMedieval : Mod
    {

        public VFEMedieval(ModContentPack content) : base(content)
        {
            harmonyInstance = HarmonyInstance.Create("OskarPotocki.VFEMedieval");
        }

        public static HarmonyInstance harmonyInstance;

    }

}
