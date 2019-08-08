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

        public List<string> shieldTags;
        public bool useDeflectMetalEffect;
        public List<BodyPartGroupDef> coveredBodyPartGroups;
        public GraphicData offHandGraphicData;
        public HoldOffsetSet offHandHoldOffset;

    }

}
