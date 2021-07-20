using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace VFEMedieval
{
    public class IncidentWorker_BlackPlague : IncidentWorker_DiseaseHuman
    {
        protected override IEnumerable<Pawn> ActualVictims(IncidentParms parms)
        {
            yield return base.ActualVictims(parms).RandomElement();
            yield break;
        }
    }
}