﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using RimWorld.BaseGen;
using Harmony;

namespace VFEMedieval
{

    public class VFEResolveParams
    {

        public const string Name = "VFEMResolveParams";

        public ThingDef edgeWallDef;
        public float? towerRadius;
        public bool? symmetricalSandbags;

    }

}
