using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEMedieval
{

    public class ThingDefExtension : DefModExtension
    {

        public static readonly ThingDefExtension defaultValues = new ThingDefExtension();

        // For weapons
        public bool usableWithShields = false;

        // For apparel
        public bool useFactionColour = false;
        public bool overrideKindDefApparelColour = false;

    }

}
