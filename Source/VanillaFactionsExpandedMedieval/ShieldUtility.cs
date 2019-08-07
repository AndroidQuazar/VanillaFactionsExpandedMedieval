using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaFactionsExpandedMedieval
{

    public static class ShieldUtility
    {

        public static int HandCount(this Pawn pawn)
        {
            int count = 0;
            var hediffSet = pawn.health.hediffSet;

            // Go through each manipulation limb and count any outside segments (i.e. anything that isn't a bone) that aren't missing
            foreach (var manipCore in pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbCore))
                count += manipCore.GetChildParts(BodyPartTagDefOf.ManipulationLimbSegment).Count(p => p.depth == BodyPartDepth.Outside && !hediffSet.PartIsMissing(p) || hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(p));

            return count;
        }

        public static bool CanUseShields(this Pawn p) => p.HandCount() > 1;

        public static bool IsShield(this Thing thing, out CompShield shieldComp)
        {
            shieldComp = thing.TryGetComp<CompShield>();
            return shieldComp != null;
        }

        public static bool IsShield(this ThingDef tDef)
        {
            return typeof(ThingWithComps).IsAssignableFrom(tDef.thingClass) && tDef.HasComp(typeof(CompShield));
        }

        public static ThingWithComps OffHandShield(this Pawn_EquipmentTracker equipment)
        {
            // Get the first shield that the pawn has equipped which isn't in the primary slot
            return equipment.AllEquipmentListForReading.FirstOrDefault(t => equipment.Primary != t && t.def.IsShield());
        }

        public static void MakeRoomForShield(this Pawn_EquipmentTracker equipment, ThingWithComps eq)
        {
            if (eq.def.equipmentType == EquipmentType.Primary && equipment.OffHandShield() != null)
            {
                ThingWithComps thingWithComps;
                if (equipment.TryDropEquipment(equipment.OffHandShield(), out thingWithComps, equipment.pawn.Position, true))
                {
                    if (thingWithComps != null)
                    {
                        thingWithComps.SetForbidden(false, true);
                    }
                }
                else
                {
                    Log.Error(equipment.pawn + " couldn't make room for shield " + eq, false);
                }
            }
        }

        public static void AddShield(this Pawn_EquipmentTracker equipment, ThingWithComps newEq)
        {
            if (newEq.def.equipmentType == EquipmentType.Primary && equipment.OffHandShield() != null)
            {
                Log.Error(string.Concat(new object[]
                {
                    "Pawn ",
                    equipment.pawn.LabelCap,
                    " got shield ",
                    newEq,
                    " while already having shield ",
                    equipment.Primary
                }), false);
                return;
            }
            ((ThingOwner<ThingWithComps>)NonPublicFields.Pawn_EquipmentTracker_equipment.GetValue(equipment)).TryAdd(newEq, true);
        }

    }

}
