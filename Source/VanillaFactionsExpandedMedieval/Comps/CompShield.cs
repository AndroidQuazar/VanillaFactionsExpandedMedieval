using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaFactionsExpandedMedieval
{

    public class CompShield : ThingComp
    {

        public CompProperties_Shield Props => (CompProperties_Shield)props;

        public Pawn EquippingPawn
        {
            get
            {
                if (ParentHolder is Pawn_EquipmentTracker equipment)
                    return equipment.pawn;
                return null;
            }
        }

        public bool UsableNow
        {
            get
            {
                if (EquippingPawn != null)
                {
                    // Get pawn's primary weapon and check if it is flagged to be usable with shields
                    var primary = EquippingPawn.equipment.Primary;
                    if (primary != null && !primary.def.IsShield())
                    {
                        var thingDefExtension = primary.def.GetModExtension<ThingDefExtension>() ?? ThingDefExtension.defaultValues;
                        return thingDefExtension.usableWithShields;
                    }
                }

                // No pawn or primary, or the primary is a shield, therefore can be used
                return true;
            }
        }

        public bool CoversBodyPart(BodyPartRecord partRec)
        {
            // Go through each covered body part group in Props and each body part group within partRec; return if there are any matches
            return Props.coveredBodyPartGroups.Any(p => partRec.groups.Any(p2 => p == p2));
        }

    }

}
