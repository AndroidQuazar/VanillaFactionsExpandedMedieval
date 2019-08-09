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

    public class JobDriver_FillWineBarrel : JobDriver
    {
        protected CompWineFermenter WineFermenter
        {
            get
            {
                return job.GetTarget(WineFermenterInd).Thing.TryGetComp<CompWineFermenter>();
            }
        }

        protected Thing Must
        {
            get
            {
                return this.job.GetTarget(MustInd).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.WineFermenter.parent;
            Job job = this.job;
            bool result;
            if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
            {
                pawn = this.pawn;
                target = this.Must;
                job = this.job;
                result = pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
            }
            else
            {
                result = false;
            }
            return result;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(WineFermenterInd);
            this.FailOnBurningImmobile(WineFermenterInd);
            base.AddEndCondition(() => (this.WineFermenter.SpaceLeftForMust > 0) ? JobCondition.Ongoing : JobCondition.Succeeded);
            yield return Toils_General.DoAtomic(delegate
            {
                this.job.count = this.WineFermenter.SpaceLeftForMust;
            });
            Toil reserveMust = Toils_Reserve.Reserve(MustInd, 1, -1, null);
            yield return reserveMust;
            yield return Toils_Goto.GotoThing(MustInd, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(MustInd).FailOnSomeonePhysicallyInteracting(MustInd);
            yield return Toils_Haul.StartCarryThing(MustInd, false, true, false).FailOnDestroyedNullOrForbidden(MustInd);
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveMust, MustInd, TargetIndex.None, true, null);
            yield return Toils_Goto.GotoThing(WineFermenterInd, PathEndMode.Touch);
            yield return Toils_General.Wait(Duration, TargetIndex.None).FailOnDestroyedNullOrForbidden(MustInd).FailOnDestroyedNullOrForbidden(WineFermenterInd).FailOnCannotTouch(WineFermenterInd, PathEndMode.Touch).WithProgressBarToilDelay(WineFermenterInd, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    this.WineFermenter.AddMust(this.Must);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }

        private const TargetIndex WineFermenterInd = TargetIndex.A;

        private const TargetIndex MustInd = TargetIndex.B;

        private const int Duration = 200;
    }

}
