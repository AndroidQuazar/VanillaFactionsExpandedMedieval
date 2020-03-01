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

    public class CompProperties_IngestedThoughtFromQuality : CompProperties
    {

        public CompProperties_IngestedThoughtFromQuality()
        {
            compClass = typeof(CompIngestedThoughtFromQuality);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            // Parent def does not have CompQuality
            if (!parentDef.HasComp(typeof(CompQuality)))
                yield return $"{parentDef} does not have CompQuality.";

            // Ingested thought is not a memory
            if (!ingestedThought.IsMemory)
                yield return $"{ingestedThought}'s thoughtClass is not a Thought_Memory.";
        }

        public ThoughtDef ingestedThought;

    }

}
