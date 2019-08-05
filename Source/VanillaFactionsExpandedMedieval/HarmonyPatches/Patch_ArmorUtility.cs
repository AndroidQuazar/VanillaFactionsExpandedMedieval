using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VanillaFactionsExpandedMedieval
{

    public static class Patch_ArmorUtility
    {

        [HarmonyPatch(typeof(ArmorUtility), nameof(ArmorUtility.GetPostArmorDamage))]
        public static class GetPostArmorDamage
        {

            public static bool Prefix(Pawn pawn, float amount, float armorPenetration, BodyPartRecord part, ref DamageDef damageDef, out bool deflectedByMetalArmor, out bool diminishedByMetalArmor, ref float __result)
            {
                deflectedByMetalArmor = false;
                diminishedByMetalArmor = false;

                // Apply shield damage reduction before apparel damage reduction
                if (damageDef.armorCategory != null)
                {
                    var armourRating = damageDef.armorCategory.armorRatingStat;
                    if (pawn.equipment != null)
                    {
                        // Multiple shields? Why not I guess
                        var shields = pawn.equipment.AllEquipmentListForReading.Where(t => t.IsShield(out CompShield sC) && sC.UsableNow);
                        foreach (var shield in shields)
                        {
                            var shieldComp = shield.TryGetComp<CompShield>();
                            if (shieldComp.CoversBodyPart(part))
                            {
                                float prevAmount = amount;
                                NonPublicMethods.ArmorUtility_ApplyArmor(ref amount, armorPenetration, shield.GetStatValue(armourRating), shield, ref damageDef, pawn, out bool metalArmour);

                                // Deflected
                                if (amount < 0.001f)
                                {
                                    deflectedByMetalArmor = metalArmour;
                                    __result = 0;
                                    return false;
                                }

                                // Diminished
                                if (amount < prevAmount)
                                    diminishedByMetalArmor = metalArmour;
                            }
                        }
                    }
                }

                return true;
            }

        }

    }

}
