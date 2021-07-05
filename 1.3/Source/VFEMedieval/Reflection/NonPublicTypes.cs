using System;
using Verse;

namespace VFEMedieval
{
    public static class NonPublicTypes
    {
        [StaticConstructorOnStartup]
        public static class RimCuisine2
        {
            static RimCuisine2()
            {
                if (ModCompatibilityCheck.RimCuisine2BottlingAndBooze)
                {
                    RCBBDefOf = GenTypes.GetTypeInAnyAssembly("RimCuisineBBDrugPolicies.RCBBDefOf", "RimCuisineBBDrugPolicies");
                }
            }

            public static Type RCBBDefOf;
        }
    }
}