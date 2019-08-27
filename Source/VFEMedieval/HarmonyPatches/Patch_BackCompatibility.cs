using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFEMedieval
{

    public static class Patch_BackCompatibility
    {

        [HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.BackCompatibleDefName))]
        public static class BackCompatibleDefName
        {

            public static void Prefix(Type defType, ref string defName)
            {
                // Replace RimCuisine grapes, must and wine with our versions
                if (defType == typeof(ThingDef))
                {
                    if (defName == "RC2_PlantGrapes")
                        defName = "VFEM_Plant_Grape";
                    else if (defName == "RC2_RawGrapes")
                        defName = "VFEM_RawGrapes";
                    else if (defName == "RC2_GrapeMust")
                        defName = "VFEM_Must";
                    else if (defName == "RC2_WineFermentingBarrel")
                        defName = "VFEM_WineBarrel";
                    else if (defName == "RC2_Wine")
                        defName = "VFEM_Wine";
                }
                else if (defType == typeof(RecipeDef))
                {
                    if (defName == "RC2_Make_GrapeMust")
                        defName = "VFEM_Make_Must";
                }
            }

        }

    }

}
