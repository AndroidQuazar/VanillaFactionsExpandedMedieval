using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEMedieval
{

    [StaticConstructorOnStartup]
    public static class StaticConstructorClass
    {

        static StaticConstructorClass()
        {
            PawnShieldGenerator.Reset();
            foreach (var tDef in DefDatabase<ThingDef>.AllDefs)
            {
                // Implied stuffProps for stone chunks
                if (tDef.IsWithinCategory(ThingCategoryDefOf.StoneChunks) && !tDef.butcherProducts.NullOrEmpty() && tDef.butcherProducts.FirstOrDefault(t => t.thingDef.IsStuff)?.thingDef is ThingDef firstStuffProduct)
                    ResolveImpliedStoneChunkStuffProperties(tDef, firstStuffProduct.stuffProps);
            }
            ResourceCounter.ResetDefs();
        }

        private static void ResolveImpliedStoneChunkStuffProperties(ThingDef stoneChunk, StuffProperties referenceProps)
        {
            stoneChunk.resourceReadoutPriority = ResourceCountPriority.Middle;
            stoneChunk.stuffProps = new StuffProperties()
            {
                stuffAdjective = referenceProps.stuffAdjective,
                commonality = referenceProps.commonality,
                categories = new List<StuffCategoryDef>() { StuffCategoryDefOf.Rocks },
                smeltable = referenceProps.smeltable,
                statOffsets = new List<StatModifier>(referenceProps.statOffsets),
                statFactors = new List<StatModifier>(referenceProps.statFactors),
                color = referenceProps.color,
                constructEffect = referenceProps.constructEffect,
                appearance = referenceProps.appearance,
                soundImpactStuff = referenceProps.soundImpactStuff,
                soundMeleeHitSharp = referenceProps.soundMeleeHitSharp,
                soundMeleeHitBlunt = referenceProps.soundMeleeHitBlunt
            };
            var statFactors = stoneChunk.stuffProps.statFactors;

            ModifyStatModifier(ref statFactors, StatDefOf.WorkToMake, ToStringNumberSense.Factor, factor: 1.5f);
            ModifyStatModifier(ref statFactors, StatDefOf.WorkToBuild, ToStringNumberSense.Factor, factor: 1.5f);
        }

        private static void ModifyStatModifier(ref List<StatModifier> modifierList, StatDef stat, ToStringNumberSense mode, float offset = 0, float factor = 1)
        {
            if (modifierList.FirstOrDefault(s => s.stat == stat) is StatModifier meleeCooldownFactor)
                meleeCooldownFactor.value = (meleeCooldownFactor.value + offset) * factor;
            else
                modifierList.Add(new StatModifier() { stat = stat, value = ((mode == ToStringNumberSense.Factor ? 1 : 0) + offset) * factor });
        }

    }

}
