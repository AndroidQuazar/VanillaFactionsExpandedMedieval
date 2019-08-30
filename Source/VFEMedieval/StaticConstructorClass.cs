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
            foreach (var tDef in DefDatabase<ThingDef>.AllDefs)
            {
                // Implied stuffProps for stone chunks
                if (tDef.IsWithinCategory(ThingCategoryDefOf.StoneChunks) && !tDef.butcherProducts.NullOrEmpty() && tDef.butcherProducts.FirstOrDefault(t => t.thingDef.IsStuff)?.thingDef is ThingDef firstStuffProduct)
                    ResolveImpliedStoneChunkStuffProperties(tDef, firstStuffProduct.stuffProps);
            }
            ResourceCounter.ResetDefs();
            MedievalTournamentUtility.SetCache();
        }

        private static void ResolveImpliedStoneChunkStuffProperties(ThingDef stoneChunk, StuffProperties referenceProps)
        {
            stoneChunk.resourceReadoutPriority = ResourceCountPriority.Middle;
            stoneChunk.stuffProps = new StuffProperties()
            {
                stuffAdjective = referenceProps.stuffAdjective,
                commonality = referenceProps.commonality,
                categories = new List<StuffCategoryDef>() { StuffCategoryDefOf.VFEM_StoneChunks },
                smeltable = referenceProps.smeltable,
                statOffsets = new List<StatModifier>(),
                statFactors = new List<StatModifier>(),
                color = referenceProps.color,
                constructEffect = referenceProps.constructEffect,
                appearance = referenceProps.appearance,
                soundImpactStuff = referenceProps.soundImpactStuff,
                soundMeleeHitSharp = referenceProps.soundMeleeHitSharp,
                soundMeleeHitBlunt = referenceProps.soundMeleeHitBlunt
            };

            var chunkProps = stoneChunk.stuffProps;
            foreach (var statOffset in referenceProps.statOffsets)
                chunkProps.statOffsets.Add(new StatModifier() { stat = statOffset.stat, value = statOffset.value });
            foreach (var statFactor in referenceProps.statFactors)
                chunkProps.statFactors.Add(new StatModifier() { stat = statFactor.stat, value = statFactor.value });

            ModifyStatModifier(ref chunkProps.statFactors, StatDefOf.WorkToMake, ToStringNumberSense.Factor, factor: 1.5f);
            ModifyStatModifier(ref chunkProps.statFactors, StatDefOf.WorkToBuild, ToStringNumberSense.Factor, factor: 1.5f);
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
