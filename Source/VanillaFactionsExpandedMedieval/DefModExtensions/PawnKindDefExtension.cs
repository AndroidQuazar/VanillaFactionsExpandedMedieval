using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEMedieval
{

    public class PawnKindDefExtension : DefModExtension
    {

        public static readonly PawnKindDefExtension defaultValues = new PawnKindDefExtension();

        public List<string> shieldTags;
        public FloatRange shieldMoney;

    }

}
