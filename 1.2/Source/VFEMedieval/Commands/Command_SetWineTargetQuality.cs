using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEMedieval
{

    public class Command_SetTargetWineQuality : Command
    {

        public Command_SetTargetWineQuality()
        {
            QualityCategory? targetQuality = null;
            bool multipleSelected = false;

            // Determine if multiple barrels are selected that have different target qualities
            var selectedObjects = Find.Selector.SelectedObjectsListForReading;
            for (int i = 0; i < selectedObjects.Count; i++)
            {
                var obj = selectedObjects[i];
                if (obj is ThingWithComps thing && thing.TryGetComp<CompWineFermenter>() is CompWineFermenter wineFermenter)
                {
                    if (targetQuality.HasValue && targetQuality.Value != wineFermenter.targetQuality)
                    {
                        multipleSelected = true;
                        break;
                    }
                    targetQuality = wineFermenter.targetQuality;
                }
            }

            // Set gizmo label depending on whether or not multiple barrels with varying target qualities were selected
            if (multipleSelected)
                defaultLabel = "VanillaFactionsExpanded.TargetWineQualityMulti".Translate();
            else
                defaultLabel = "VanillaFactionsExpanded.TargetWineQuality".Translate(targetQuality.Value.GetLabel());
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            if (wineFermenters == null)
                wineFermenters = new List<CompWineFermenter>();
            wineFermenters.Add(wineFermenter);

            var floatMenuOptions = new List<FloatMenuOption>();
            foreach (QualityCategory quality in Enum.GetValues(typeof(QualityCategory)))
                floatMenuOptions.Add(new FloatMenuOption(quality.GetLabel(), () => wineFermenters.ForEach(f => f.targetQuality = quality)));

            Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
        }

        public override bool InheritInteractionsFrom(Gizmo other)
        {
            if (wineFermenters == null)
                wineFermenters = new List<CompWineFermenter>();
            wineFermenters.Add(((Command_SetTargetWineQuality)other).wineFermenter);
            return false;
        }

        public CompWineFermenter wineFermenter;
        private List<CompWineFermenter> wineFermenters;

    }

}
