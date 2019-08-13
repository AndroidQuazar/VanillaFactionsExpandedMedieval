using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEMedieval
{

    public class IncidentDefExtension : DefModExtension
    {

        public static readonly IncidentDefExtension defaultValues = new IncidentDefExtension();

        public FactionDef forcedFaction;
        public IntRange forcedPointsRange = IntRange.zero;
        public RaidStrategyDef forcedStrategy;

    }

}
