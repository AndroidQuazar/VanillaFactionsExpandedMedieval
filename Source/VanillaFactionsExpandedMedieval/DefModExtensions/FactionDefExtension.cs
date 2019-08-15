using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEMedieval
{

    public class FactionDefExtension : DefModExtension
    {

        public static readonly FactionDefExtension defaultValues = new FactionDefExtension();

        public bool customSieges;
        public List<string> artilleryBuildingTags = new List<string>() { "Artillery_BaseDestroyer" };
        public ThingDef siegeMealDef = RimWorld.ThingDefOf.MealSurvivalPack;
        public string settlementGenerationSymbol;

    }

}
