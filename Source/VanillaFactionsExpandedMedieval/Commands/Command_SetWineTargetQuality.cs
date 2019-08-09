using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaFactionsExpandedMedieval
{

    public class Command_SetTargetWineQuality : Command
    {

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
