﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;

namespace VFEMedieval
{

    public class JobDriver_EquipShield : JobDriver_Equip
    {

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    ThingWithComps equipmentStack = (ThingWithComps)job.targetA.Thing;
                    ThingWithComps equippedThing;
                    if (equipmentStack.def.stackLimit > 1 && equipmentStack.stackCount > 1)
                    {
                        equippedThing = (ThingWithComps)equipmentStack.SplitOff(1);
                    }
                    else
                    {
                        equippedThing = equipmentStack;
                        equippedThing.DeSpawn(DestroyMode.Vanish);
                    }
                    ShieldUtility.MakeRoomForShield(pawn.equipment, equippedThing);
                    ShieldUtility.AddShield(pawn.equipment, equippedThing);
                    if (equipmentStack.def.soundInteract != null)
                    {
                        equipmentStack.def.soundInteract.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

    }

}
