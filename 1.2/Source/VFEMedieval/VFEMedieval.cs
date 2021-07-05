using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFEMedieval
{

    public class VFEMedieval : Mod
    {

        public VFEMedieval(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony("OskarPotocki.VFEMedieval");
        }

        public static Harmony harmonyInstance;

    }

}
