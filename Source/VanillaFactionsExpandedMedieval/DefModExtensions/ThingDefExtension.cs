using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaFactionsExpandedMedieval
{

    public class ThingDefExtension : DefModExtension
    {

        public static readonly ThingDefExtension defaultValues = new ThingDefExtension();

        public bool usableWithShields = false;

    }

}
