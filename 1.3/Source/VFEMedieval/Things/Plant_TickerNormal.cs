using HarmonyLib;
using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class Plant_TickerNormal : Plant 
    {
        public override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(2000))
            {
                base.TickLong();
            }
        }
    }
}