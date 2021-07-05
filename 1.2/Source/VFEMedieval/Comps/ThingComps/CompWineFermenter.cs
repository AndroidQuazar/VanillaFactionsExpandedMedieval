using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFEMedieval
{

    [StaticConstructorOnStartup]
    public class CompWineFermenter : ThingComp
    {

        private const float MinIdealTemperature = 7;
        private static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);
        private static readonly Color BarZeroProgressColour = new Color(0.7f, 0.22f, 0.22f);
        private static readonly Color BarFermentedColour = new Color(0.4f, 0.22f, 0.22f);
        private static readonly Material BarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f), false);

        private int mustCount;
        private int _ageTicks;
        private Material barFilledCachedMat;
        public QualityCategory targetQuality = QualityCategory.Normal;

        public CompProperties_WineFermenter Props => (CompProperties_WineFermenter)props;

        public CompTemperatureRuinable TemperatureRuinableComp => parent.TryGetComp<CompTemperatureRuinable>();

        public int SpaceLeftForMust => Fermented ? 0 : Props.mustCapacity - mustCount;

        private bool Empty => mustCount <= 0;

        public int AgeTicks
        {
            get => _ageTicks;
            set
            {
                if (_ageTicks == value)
                    return;
                _ageTicks = value;
                barFilledCachedMat = null;
            }
        }

        private float AgeDays => (float)AgeTicks / GenDate.TicksPerDay;

        private Material BarFilledMat
        {
            get
            {
                if (barFilledCachedMat == null)
                    barFilledCachedMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.Lerp(BarZeroProgressColour, BarFermentedColour, Progress));
                return barFilledCachedMat;
            }
        }

        private QualityCategory CurrentQuality
        {
            get
            {
                if (AgeDays < Props.poorQualityAgeDaysThreshold)
                    return QualityCategory.Awful;
                if (AgeDays < Props.normalQualityAgeDaysThreshold)
                    return QualityCategory.Poor;
                if (AgeDays < Props.goodQualityAgeDaysThreshold)
                    return QualityCategory.Normal;
                if (AgeDays < Props.excellentQualityAgeDaysThreshold)
                    return QualityCategory.Good;
                if (AgeDays < Props.masterworkQualityAgeDaysThreshold)
                    return QualityCategory.Excellent;
                if (AgeDays < Props.legendaryQualityAgeDaysThreshold)
                    return QualityCategory.Masterwork;
                return QualityCategory.Legendary;
            }
        }

        public float DaysToReachTargetQuality
        {
            get
            {
                switch (targetQuality)
                {
                    case QualityCategory.Awful:
                        return Props.awfulQualityAgeDaysThreshold;
                    case QualityCategory.Poor:
                        return Props.poorQualityAgeDaysThreshold;
                    case QualityCategory.Normal:
                        return Props.normalQualityAgeDaysThreshold;
                    case QualityCategory.Good:
                        return Props.goodQualityAgeDaysThreshold;
                    case QualityCategory.Excellent:
                        return Props.excellentQualityAgeDaysThreshold;
                    case QualityCategory.Masterwork:
                        return Props.masterworkQualityAgeDaysThreshold;
                    default:
                        return Props.legendaryQualityAgeDaysThreshold;
                }
            }
        }

        public int TicksToReachTargetQuality => Mathf.RoundToInt(DaysToReachTargetQuality * GenDate.TicksPerDay);

        public float Progress
        {
            get => (TicksToReachTargetQuality > 0) ? ((float)AgeTicks / TicksToReachTargetQuality) : 1;
            set => AgeTicks = Mathf.RoundToInt(TicksToReachTargetQuality * value);
        }


        private float CurrentTempProgressSpeedFactor
        {
            get
            {
                if (TemperatureRuinableComp != null)
                {
                    var tempRuinProps = TemperatureRuinableComp.Props;
                    float ambientTemperature = parent.AmbientTemperature;
                    if (ambientTemperature < tempRuinProps.minSafeTemperature)
                    {
                        return 0.1f;
                    }
                    if (ambientTemperature < MinIdealTemperature)
                    {
                        return GenMath.LerpDouble(tempRuinProps.minSafeTemperature, MinIdealTemperature, 0.1f, 1f, ambientTemperature);
                    }
                    return 1f;
                }
                return 1;
            }
        }

        private int EstimatedTicksLeft => Mathf.Max(Mathf.RoundToInt((1 - Progress) * DaysToReachTargetQuality * GenDate.TicksPerDay / CurrentTempProgressSpeedFactor), 0);

        public bool Fermented => !Empty && Progress >= 1;

        private void Reset()
        {
            mustCount = 0;
            AgeTicks = 0;
        }

        public void AddMust(int count)
        {
            if (TemperatureRuinableComp != null)
                TemperatureRuinableComp.Reset();

            if (Fermented)
            {
                Log.Warning("Tried to add must to a barrel full of wine. Colonists should take the wine first.", false);
                return;
            }

            int num = Mathf.Min(count, Props.mustCapacity - mustCount);
            if (num <= 0)
                return;

            AgeTicks = Mathf.RoundToInt(GenMath.WeightedAverage(0f, num, Progress, mustCount));
            mustCount += num;
        }

        public void AddMust(Thing must)
        {
            int num = Mathf.Min(must.stackCount, Props.mustCapacity - mustCount);
            if (num > 0)
            {
                this.AddMust(num);
                must.SplitOff(num).Destroy(DestroyMode.Vanish);
            }
        }

        public Thing TakeOutWine()
        {
            if (!Fermented)
            {
                Log.Warning("Tried to get wine but it's not yet fermented.", false);
                return null;
            }
            var wine = ThingMaker.MakeThing(ThingDefOf.VFEM_Wine, null);
            wine.stackCount = mustCount;
            if (wine.TryGetComp<CompQuality>() is CompQuality qualityComp)
                qualityComp.SetQuality(CurrentQuality, ArtGenerationContext.Colony);
            Reset();
            return wine;
        }

        public override void CompTickRare()
        {
            if (!Empty)
                AgeTicks += Mathf.RoundToInt(GenTicks.TickRareInterval * CurrentTempProgressSpeedFactor);
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (!Empty)
            {
                var drawPos = parent.DrawPos + new Vector3(0, 0.046875f, 0.25f);
                GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest()
                {
                    center = drawPos,
                    size = BarSize,
                    fillPercent = (float)mustCount / Props.mustCapacity,
                    filledMat = BarFilledMat,
                    unfilledMat = BarUnfilledMat,
                    margin = 0.1f,
                    rotation = Rot4.North
                });
            }
        }

        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "RuinedByTemperature")
                Reset();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_SetTargetWineQuality()
            {
                defaultDesc = "VanillaFactionsExpanded.TargetWineQuality_Description".Translate(),
                icon = ThingDefOf.VFEM_Wine.uiIcon,
                wineFermenter = this,
            };

            // Debugging gizmos
            if (Prefs.DevMode && !Empty)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Debug: Age 1 day",
                    action = () => AgeTicks += GenDate.TicksPerDay
                };
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (!Empty)
            {
                if (!TemperatureRuinableComp.Ruined)
                {
                    if (AgeDays >= Props.awfulQualityAgeDaysThreshold)
                        stringBuilder.AppendLine($"{"VanillaFactionsExpanded.ContainsWine".Translate(mustCount, Props.mustCapacity)} ({"QualityIs".Translate(CurrentQuality.GetLabel()).ToLower()})");
                    else
                        stringBuilder.AppendLine("VanillaFactionsExpanded.ContainsMust".Translate(mustCount, Props.mustCapacity, AgeTicks.ToStringTicksToPeriod()));
                }
                if (Fermented)
                {
                    stringBuilder.AppendLine("Fermented".Translate());
                }
                else
                {
                    stringBuilder.AppendLine("FermentationProgress".Translate(Progress.ToStringPercent(), EstimatedTicksLeft.ToStringTicksToPeriod()));
                    if (CurrentTempProgressSpeedFactor != 1f)
                    {
                        stringBuilder.AppendLine("FermentationBarrelOutOfIdealTemperature".Translate(CurrentTempProgressSpeedFactor.ToStringPercent()));
                    }
                }
            }
            stringBuilder.AppendLine("Temperature".Translate() + ": " + parent.AmbientTemperature.ToStringTemperature("F0"));
            stringBuilder.AppendLine($"{"IdealFermentingTemperature".Translate()}: {MinIdealTemperature.ToStringTemperature("F0")} ~ {TemperatureRuinableComp.Props.maxSafeTemperature.ToStringTemperature("F0")}");
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref mustCount, "mustCount");
            Scribe_Values.Look(ref _ageTicks, "ageTicks");
            Scribe_Values.Look(ref targetQuality, "targetQuality", QualityCategory.Normal);

            base.PostExposeData();
        }

    }

}
