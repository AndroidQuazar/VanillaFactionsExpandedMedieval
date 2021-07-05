using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VFEMedieval
{
    public class CompProperties_IngestedThoughtFromQuality : CompProperties
    {
        public ThoughtDef ingestedThought;

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
    }
}