using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaFactionsExpandedMedieval
{

    public class CompProperties_Shield : CompProperties
    {

        public CompProperties_Shield()
        {
            compClass = typeof(CompShield);
        }

        public List<BodyPartGroupDef> coveredBodyPartGroups = new List<BodyPartGroupDef>();

    }

}
