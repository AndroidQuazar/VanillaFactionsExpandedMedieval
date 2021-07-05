using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEMedieval
{

    public static class MedievalTournamentUtility
    {

        private const float ArcheryEffectivenessNormaliser = 12.6f;
        private const float MeleeEffectivenessNormaliser = 15.1f;
        private const float JoustEffectivenessNormaliser = 8.4f;
        public const float DisasterChancePerRound = 0.04f;
        public static readonly IntRange QuestSiteTournamentTimeoutDaysRange = new IntRange(7, 15);
        public static readonly IntRange CompetitorCountRange = new IntRange(3, 6);
        public static readonly SimpleCurve EffectivenessAdvantageToWinChanceCurve = new SimpleCurve()
        {
            new CurvePoint(-5, 0.02f),
            new CurvePoint(-4, 0.04f),
            new CurvePoint(-3, 0.07f),
            new CurvePoint(-2, 0.13f),
            new CurvePoint(-1, 0.25f),
            new CurvePoint(0, 0.5f),
            new CurvePoint(1, 0.75f),
            new CurvePoint(2, 0.87f),
            new CurvePoint(3, 0.93f),
            new CurvePoint(4, 0.96f),
            new CurvePoint(5, 0.98f)
        };

        private const float ExistingPawnSelectChance = 0.7f;

        private static float archeryDistance;

        public static void SetCache()
        {
            var verbProps = ThingDefOf.Bow_Great.Verbs.First(v => v.isPrimary);
            archeryDistance = Mathf.Round(Mathf.Lerp(verbProps.minRange, verbProps.range, 0.75f));
        }

        private static float MentalBreakThresholdMultiplierFor(Pawn pawn, float weight)
        {
            float breakThreshold = pawn.GetStatValue(StatDefOf.MentalBreakThreshold);
            float defaultValue = StatDefOf.MentalBreakThreshold.defaultBaseValue;
            if (breakThreshold < defaultValue)
                return Mathf.Lerp(1 + weight, 1, breakThreshold / defaultValue);
            return 1 / (1 + weight * (breakThreshold - defaultValue));
        }

        private static float BaseMeleeEffectivenessFor(Pawn pawn)
        {
            float accuracy = pawn.GetStatValue(StatDefOf.MeleeHitChance);
            return Mathf.Pow(accuracy, 3.6f) * MentalBreakThresholdMultiplierFor(pawn, 0.1f) * MeleeEffectivenessNormaliser;
        }

        private static float BaseJoustEffectivenessFor(Pawn pawn)
        {
            float accuracy = pawn.GetStatValue(StatDefOf.MeleeHitChance);
            float tameChance = pawn.GetStatValue(StatDefOf.TameAnimalChance);
            return (Mathf.Pow(accuracy, 3.6f) * JoustEffectivenessNormaliser + Mathf.Pow(tameChance, 0.625f) * JoustEffectivenessNormaliser) * MentalBreakThresholdMultiplierFor(pawn, 0.1f);
        }

        private static float BaseArcheryEffectivenessFor(Pawn pawn)
        {
            float accuracy = ShotReport.HitFactorFromShooter(pawn, archeryDistance);
            return accuracy * MentalBreakThresholdMultiplierFor(pawn, 0.25f) * ArcheryEffectivenessNormaliser;
        }

        public static float TournamentEffectivenessFor(Pawn pawn, TournamentCategoryDef category)
        {
            if (category == TournamentCategoryDefOf.VFEM_Melee)
                return BaseMeleeEffectivenessFor(pawn);
            if (category == TournamentCategoryDefOf.VFEM_Jousting)
                return BaseJoustEffectivenessFor(pawn);
            if (category == TournamentCategoryDefOf.VFEM_Archery)
                return BaseArcheryEffectivenessFor(pawn);
            throw new NotImplementedException();
        }

        public static bool CanParticipate(Pawn pawn, TournamentCategoryDef category)
        {
            if (pawn.Downed || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                return false;

            if (category == TournamentCategoryDefOf.VFEM_Melee)
                return !StatDefOf.MeleeHitChance.Worker.IsDisabledFor(pawn);
            if (category == TournamentCategoryDefOf.VFEM_Jousting)
                return !StatDefOf.MeleeHitChance.Worker.IsDisabledFor(pawn) && !StatDefOf.TameAnimalChance.Worker.IsDisabledFor(pawn);
            if (category == TournamentCategoryDefOf.VFEM_Archery)
                return !StatDefOf.ShootingAccuracyPawn.Worker.IsDisabledFor(pawn);

            throw new NotImplementedException();
        }

        public static string TournamentEffectivenessString(float effectiveness)
        {
            string resultStr;
            if (effectiveness < 2)
                resultStr = "QualityCategory_Awful".Translate();
            else if (effectiveness < 4)
                resultStr = "QualityCategory_Poor".Translate();
            else if (effectiveness < 6)
                resultStr = "Average".Translate();
            else if (effectiveness < 8)
                resultStr = "QualityCategory_Good".Translate();
            else
                resultStr = "QualityCategory_Excellent".Translate();
            return resultStr.UncapitalizeFirst();
        }

        public static string ParticipantOptionText(Pawn pawn, TournamentCategoryDef category)
        {
            string pawnName = pawn.Name.ToStringShort;
            if (CanParticipate(pawn, category))
                return $"{pawnName} ({"VanillaFactionsExpanded.PredictedPerformance".Translate()}: {TournamentEffectivenessString(TournamentEffectivenessFor(pawn, category))})";
            return $"{pawnName} ({"VanillaFactionsExpanded.CannotParticipate".Translate()})";
        }

        public static void GroupParticipants(List<Pawn> inPawnList, TournamentCategoryDef category, List<Pawn> outParticipants, List<Pawn> outNonParticipants)
        {
            for (int i = 0; i < inPawnList.Count(); i++)
            {
                var pawn = inPawnList[i];
                if (CanParticipate(pawn, category))
                    outParticipants.Add(pawn);
                else
                    outNonParticipants.Add(pawn);
            }
            outParticipants.SortByDescending(p => TournamentEffectivenessFor(p, category));
        }

        public static Pawn GenerateNewCompetitor(TournamentCategoryDef category, Faction faction)
        {
            return PawnGenerator.GeneratePawn(new PawnGenerationRequest(faction.RandomPawnKind(), faction, forceGenerateNewPawn: true, validatorPreGear: p => CanParticipate(p, category)));
        }

        public static List<Pawn> GenerateCompetitors(int count, TournamentCategoryDef category, Faction faction, IEnumerable<Pawn> existingPawns = null)
        {
            var potentialCompetitors = existingPawns?.ToList() ?? null;
            var resultList = new List<Pawn>();
            for (int i = 0; i < count; i++)
            {
                if (!potentialCompetitors.NullOrEmpty() && Rand.Chance(ExistingPawnSelectChance))
                {
                    var pawn = potentialCompetitors.RandomElement();
                    resultList.Add(pawn);
                    potentialCompetitors.Remove(pawn);
                }
                else
                    resultList.Add(GenerateNewCompetitor(category, faction));
            }
            return resultList;
        }

    }

}
