using HarmonyLib;
using System.Reflection;
using Verse;

namespace VFEMedieval
{
    [StaticConstructorOnStartup]
    public static class NonPublicFields
    {
        public static FieldInfo DamageInfo_armorPenetrationInt = AccessTools.Field(typeof(DamageInfo), "armorPenetrationInt");
    }
}