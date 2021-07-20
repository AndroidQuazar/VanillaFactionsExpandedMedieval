using RimWorld;
using Verse;

namespace VFEMedieval
{
    [DefOf]
    public static class VFEM_DefOf
    {
        public static ThingDef VFEM_CobblestoneWall;
        public static ThingDef VFEM_Must;
        public static ThingDef VFEM_WineBarrel;
        public static ThingDef VFEM_Wine;
        public static ThingDef VFEM_MeleeWeapon_Claymore;

        // Vanilla defs
        public static ThingDef Bow_Great;
        public static ThingDef MeleeWeapon_LongSword;
        public static ThingDef MeleeWeapon_Gladius;

        public static HediffDef Plague;
        public static HediffDef VFEM_BlackPlague;
        public static ThingDef VFEM_Apparel_PlagueMask;
        public static FleckDef VFEM_ArrowThrowable;

        public static DesignationDef VFEM_DigTerrain;
        public static DesignationDef VFEM_FillTerrain;
        public static TerrainAffordanceDef VFEM_Moatable;

        public static SitePartDef VFE_Skirmish;
    }

    [DefOf]
    public static class VFEM_JobDefOf
    {
        public static JobDef VFEM_DigTerrain;

        public static JobDef VFEM_FillTerrain;
    }
}