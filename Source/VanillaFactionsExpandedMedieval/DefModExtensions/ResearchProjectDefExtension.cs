using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEMedieval
{

    public class ResearchProjectDefExtension : DefModExtension
    {

        public static readonly ResearchProjectDefExtension defaultValues = new ResearchProjectDefExtension();

        public List<ResearchProjectTagDef> greylistedTags;

    }

}
