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

    public class JobDriver_TakeWineOutOfWineBarrel : JobDriver
    {
        protected CompWineFermenter WineFermenter
        {
            get
            {
                return job.GetTarget(WineFermenterInd).Thing.TryGetComp<CompWineFermenter>();
            }
        }

        protected Thing Wine
        {
            get
            {
                return this.job.GetTarget(WineToHaulInd).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.WineFermenter.parent;
            Job job = this.job;
            return pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(WineFermenterInd);
            this.FailOnBurningImmobile(WineFermenterInd);
            yield return Toils_Goto.GotoThing(WineFermenterInd, PathEndMode.Touch);
            yield return Toils_General.Wait(Duration, TargetIndex.None).FailOnDestroyedNullOrForbidden(WineFermenterInd).FailOnCannotTouch(WineFermenterInd, PathEndMode.Touch).FailOn(() => !this.WineFermenter.Fermented).WithProgressBarToilDelay(WineFermenterInd, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Thing thing = this.WineFermenter.TakeOutWine();
                    GenPlace.TryPlaceThing(thing, this.pawn.Position, base.Map, ThingPlaceMode.Near, null, null);
                    StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
                    IntVec3 c;
                    if (StoreUtility.TryFindBestBetterStoreCellFor(thing, this.pawn, base.Map, currentPriority, this.pawn.Faction, out c, true))
                    {
                        this.job.SetTarget(StorageCellInd, c);
                        this.job.SetTarget(WineToHaulInd, thing);
                        this.job.count = thing.stackCount;
                    }
                    else
                    {
                        base.EndJobWith(JobCondition.Incompletable);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return Toils_Reserve.Reserve(WineToHaulInd, 1, -1, null);
            yield return Toils_Reserve.Reserve(StorageCellInd, 1, -1, null);
            yield return Toils_Goto.GotoThing(WineToHaulInd, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(WineToHaulInd, false, false, false);
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(StorageCellInd);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(StorageCellInd, carryToCell, true);
            yield break;
        }

        private const TargetIndex WineFermenterInd = TargetIndex.A;

        private const TargetIndex WineToHaulInd = TargetIndex.B;

        private const TargetIndex StorageCellInd = TargetIndex.C;

        private const int Duration = 200;

    }


}
