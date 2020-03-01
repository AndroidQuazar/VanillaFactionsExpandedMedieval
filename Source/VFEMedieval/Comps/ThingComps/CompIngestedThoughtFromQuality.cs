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

    public class CompIngestedThoughtFromQuality : ThingComp
    {

        CompProperties_IngestedThoughtFromQuality Props => (CompProperties_IngestedThoughtFromQuality)props;

        public override void PostIngested(Pawn ingester)
        {
            base.PostIngested(ingester);
            if (ingester.needs.mood != null)
            {
                var memories = ingester.needs.mood.thoughts.memories;
                var curIngestedMemory = memories.GetFirstMemoryOfDef(Props.ingestedThought);
                int quality = (int)parent.GetComp<CompQuality>().Quality;

                // Modify the existing memory if it exists
                if (curIngestedMemory != null)
                {
                    float averageIndex = (float)(curIngestedMemory.CurStageIndex + quality) / 2;
                    curIngestedMemory.SetForcedStage((quality > curIngestedMemory.CurStageIndex) ? Mathf.RoundToInt(averageIndex) : Mathf.FloorToInt(averageIndex));
                    curIngestedMemory.Renew();
                }

                // Otherwise create a new one
                else
                {
                    var ingestedMemory = ThoughtMaker.MakeThought(Props.ingestedThought, quality);
                    ingester.needs.mood.thoughts.memories.TryGainMemory(ingestedMemory);
                }
            }
                
        }

    }

}
