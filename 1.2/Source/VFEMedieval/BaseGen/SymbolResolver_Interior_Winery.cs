using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using RimWorld.BaseGen;
using HarmonyLib;

namespace VFEMedieval
{

    public class SymbolResolver_Interior_Winery : SymbolResolver
    {
        private float SpawnPassiveCoolerIfTemperatureAbove
        {
            get
            {
                return ThingDefOf.VFEM_WineBarrel.GetCompProperties<CompProperties_TemperatureRuinable>().maxSafeTemperature;
            }
        }

        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            if (map.mapTemperature.OutdoorTemp > this.SpawnPassiveCoolerIfTemperatureAbove)
            {
                ResolveParams resolveParams = rp;
                resolveParams.singleThingDef = RimWorld.ThingDefOf.PassiveCooler;
                BaseGen.symbolStack.Push("edgeThing", resolveParams);
            }
            else if (map.mapTemperature.OutdoorTemp < SpawnHeaterIfTemperatureBelow)
            {
                ThingDef singleThingDef;
                if (rp.faction == null || rp.faction.def.techLevel >= TechLevel.Industrial)
                {
                    singleThingDef = RimWorld.ThingDefOf.Heater;
                }
                else
                {
                    singleThingDef = RimWorld.ThingDefOf.Campfire;
                }
                ResolveParams resolveParams2 = rp;
                resolveParams2.singleThingDef = singleThingDef;
                BaseGen.symbolStack.Push("edgeThing", resolveParams2);
            }
            BaseGen.symbolStack.Push("VFEM_addMustToWineBarrels", rp);
            ResolveParams resolveParams3 = rp;
            resolveParams3.singleThingDef = ThingDefOf.VFEM_WineBarrel;
            resolveParams3.thingRot = new Rot4?((!Rand.Bool) ? Rot4.East : Rot4.North);
            int? fillWithThingsPadding = rp.fillWithThingsPadding;
            resolveParams3.fillWithThingsPadding = new int?((fillWithThingsPadding == null) ? 1 : fillWithThingsPadding.Value);
            BaseGen.symbolStack.Push("fillWithThings", resolveParams3);
        }

        private const float SpawnHeaterIfTemperatureBelow = 7f;
    }

}
