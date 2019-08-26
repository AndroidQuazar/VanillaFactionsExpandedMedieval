using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEMedieval
{

    public static class BlackKnightUtility
    {

        private static bool IsValidArthur(Pawn pawn)
        {
            return pawn.RaceProps.Humanlike && pawn.kindDef != PawnKindDefOf.VFE_BlackKnight && pawn.LabelShort.ToLower() == "arthur" && pawn.equipment.Primary.def == ThingDefOf.MeleeWeapon_LongSword;
        }

        private static void TryAvoidFleshWound(Pawn pawn, ref DamageInfo dinfo)
        {
            if (IsValidArthur(pawn) && dinfo.Instigator is Pawn i && i.kindDef == PawnKindDefOf.VFE_BlackKnight)
            {
                dinfo.SetAmount(0);
            }
        }

        private static void TryDoFleshWound(Pawn pawn, ref DamageInfo dinfo)
        {
            // 'tis but a scratch! Picks a random limb and guarantees that the attack will cut it off
            if (pawn.kindDef == PawnKindDefOf.VFE_BlackKnight && dinfo.Instigator is Pawn i && IsValidArthur(i))
            {
                var body = pawn.RaceProps.body;
                var limbs = body.GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbCore).Concat(body.GetPartsWithTag(BodyPartTagDefOf.MovingLimbCore)).Where(p => !pawn.health.hediffSet.PartIsMissing(p));
                if (limbs.Any())
                {
                    var limb = limbs.RandomElement();
                    dinfo.Def = DamageDefOf.VFE_CutBlackKnight;
                    dinfo.SetHitPart(limb);
                    dinfo.SetAmount(limb.def.GetMaxHealth(pawn) * (1 + dinfo.Def.overkillPctToDestroyPart.TrueMax) + 1);
                    NonPublicFields.DamageInfo_armorPenetrationInt.SetValue(dinfo, 99999);
                }
            }
        }

        public static void ModifyDamageInfo(Pawn pawn, ref DamageInfo dinfo)
        {
            TryAvoidFleshWound(pawn, ref dinfo);
            TryDoFleshWound(pawn, ref dinfo);
        }

    }

}
