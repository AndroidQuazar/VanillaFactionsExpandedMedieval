using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace VFEMedieval
{

    public class WorkGiver_FillWineBarrel : WorkGiver_Scanner
    {

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(ThingDefOf.VFEM_WineBarrel);
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var wineFermenter = t.TryGetComp<CompWineFermenter>();
            if (wineFermenter == null || wineFermenter.Fermented || wineFermenter.SpaceLeftForMust <= 0)
            {
                return false;
            }
            float ambientTemperature = wineFermenter.parent.AmbientTemperature;
            CompProperties_TemperatureRuinable compProperties = wineFermenter.TemperatureRuinableComp.Props;
            if (ambientTemperature < compProperties.minSafeTemperature + 2f || ambientTemperature > compProperties.maxSafeTemperature - 2f)
            {
                JobFailReason.Is("BadTemperature".Translate().ToLower(), null);
                return false;
            }
            if (!t.IsForbidden(pawn))
            {
                LocalTargetInfo target = t;
                if (pawn.CanReserve(target, 1, -1, null, forced))
                {
                    if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
                    {
                        return false;
                    }
                    if (this.FindMust(pawn, wineFermenter) == null)
                    {
                        JobFailReason.Is("VanillaFactionsExpanded.NoMust".Translate(), null);
                        return false;
                    }
                    return !t.IsBurning();
                }
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var wineFermenter = t.TryGetComp<CompWineFermenter>();
            Thing t2 = this.FindMust(pawn, wineFermenter);
            return new Job(JobDefOf.VFEM_FillWineBarrel, t, t2);
        }

        private Thing FindMust(Pawn pawn, CompWineFermenter wineFermenter)
        {
            Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false);
            IntVec3 position = pawn.Position;
            Map map = pawn.Map;
            ThingRequest thingReq = ThingRequest.ForDef(ThingDefOf.VFEM_Must);
            PathEndMode peMode = PathEndMode.ClosestTouch;
            TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            Predicate<Thing> validator = predicate;
            return GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
        }

    }

}
