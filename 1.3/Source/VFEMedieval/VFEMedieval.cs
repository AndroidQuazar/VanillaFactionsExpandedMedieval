using HarmonyLib;
using Verse;

namespace VFEMedieval
{
    public class VFEMedieval : Mod
    {
        public VFEMedieval(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony("OskarPotocki.VFEMedieval");
        }

        public static Harmony harmonyInstance;
    }
}