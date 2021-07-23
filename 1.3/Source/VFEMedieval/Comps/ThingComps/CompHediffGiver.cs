using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VFEMedieval
{
    public class CompProperties_HediffGiver : CompProperties
    {
        public float adjustSeverity;
        public int minTicksToAffect;
        public HediffDef hediff;
        public float radius;
        public CompProperties_HediffGiver()
        {
            this.compClass = typeof(CompHediffGiver);
        }
    }
    public class CompHediffGiver : ThingComp
    {
        private const int HEDIFF_GIVER_TICKRATE = 30;
        private Dictionary<Pawn, int> affectedPawns = new Dictionary<Pawn, int>();
        public CompProperties_HediffGiver Props => base.props as CompProperties_HediffGiver;
        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.IsHashIntervalTick(HEDIFF_GIVER_TICKRATE))
            {
                if (affectedPawns is null)
                {
                    affectedPawns = new Dictionary<Pawn, int>();
                }
                List<Pawn> touchedPawns = new List<Pawn>();
                
                foreach (var thing in GenRadial.RadialDistinctThingsAround(this.parent.Position, this.parent.Map, this.Props.radius, true))
                {
                    if (thing is Pawn pawn)
                    {
                        touchedPawns.Add(pawn);
                        if (affectedPawns.ContainsKey(pawn))
                        {
                            affectedPawns[pawn] += HEDIFF_GIVER_TICKRATE;
                        }
                        else
                        {
                            affectedPawns[pawn] = HEDIFF_GIVER_TICKRATE;
                        }
                    }
                }

                affectedPawns.RemoveAll(x => !touchedPawns.Contains(x.Key));
                foreach (var pawn in affectedPawns.Keys.ToList())
                {
                    if (affectedPawns[pawn] >= Props.minTicksToAffect)
                    {
                        affectedPawns[pawn] -= Props.minTicksToAffect;
                        HealthUtility.AdjustSeverity(pawn, Props.hediff, Props.adjustSeverity);
                    }
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref affectedPawns, "affectedPawns", LookMode.Reference, LookMode.Value, ref pawnKeys, ref intValues);
        }

        private List<Pawn> pawnKeys;
        private List<int> intValues;
    }
}