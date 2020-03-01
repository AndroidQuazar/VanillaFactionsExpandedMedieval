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

    public class TournamentCategoryDef : Def
    {

        public float commonality = 1;
        public Dictionary<SkillDef, float> skillExpGains;
        public RulePackDef rulePack;

    }

}
