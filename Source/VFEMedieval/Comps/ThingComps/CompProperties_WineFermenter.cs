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

    public class CompProperties_WineFermenter : CompProperties
    {

        public CompProperties_WineFermenter()
        {
            compClass = typeof(CompWineFermenter);
        }

        public int mustCapacity;
        public float awfulQualityAgeDaysThreshold;
        public float poorQualityAgeDaysThreshold;
        public float normalQualityAgeDaysThreshold;
        public float goodQualityAgeDaysThreshold;
        public float excellentQualityAgeDaysThreshold;
        public float masterworkQualityAgeDaysThreshold;
        public float legendaryQualityAgeDaysThreshold;

    }

}
