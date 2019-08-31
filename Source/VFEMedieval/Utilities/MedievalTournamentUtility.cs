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
        public static float archeryDisasterDamage;

        public static void SetCache()
        {
            var verbProps = ThingDefOf.Bow_Great.Verbs.First(v => v.isPrimary);
            archeryDistance = Mathf.Round(Mathf.Lerp(verbProps.minRange, verbProps.range, 0.75f));
            archeryDisasterDamage = verbProps.defaultProjectile.projectile.GetDamageAmount(1);
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

        public static float TournamentEffectivenessFor(Pawn pawn, TournamentCategory category)
        {
            switch(category)
            {
                case TournamentCategory.Melee:
                    return BaseMeleeEffectivenessFor(pawn);
                case TournamentCategory.Jousting:
                    return BaseJoustEffectivenessFor(pawn);
                case TournamentCategory.Archery:
                    return BaseArcheryEffectivenessFor(pawn);
                default:
                    throw new NotImplementedException();
            }
        }

        public static bool CanParticipate(Pawn pawn, TournamentCategory category)
        {
            bool eligible = !pawn.Downed && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            switch (category)
            {
                case TournamentCategory.Melee:
                    return eligible && !StatDefOf.MeleeHitChance.Worker.IsDisabledFor(pawn);
                case TournamentCategory.Jousting:
                    return eligible && (!StatDefOf.MeleeHitChance.Worker.IsDisabledFor(pawn) || !StatDefOf.TameAnimalChance.Worker.IsDisabledFor(pawn));
                case TournamentCategory.Archery:
                    return eligible && !StatDefOf.ShootingAccuracyPawn.Worker.IsDisabledFor(pawn);
                default:
                    throw new NotImplementedException();
            }
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

        public static string ParticipantOptionText(Pawn pawn, TournamentCategory category)
        {
            string pawnName = pawn.Name.ToStringShort;
            if (CanParticipate(pawn, category))
                return $"{pawnName} ({"VanillaFactionsExpanded.PredictedPerformance".Translate()}: {TournamentEffectivenessString(TournamentEffectivenessFor(pawn, category))})";
            return $"{pawnName} ({"VanillaFactionsExpanded.CannotParticipate".Translate()})";
        }

        public static void GroupParticipants(IEnumerable<Pawn> inPawns, TournamentCategory category, List<Pawn> outParticipants, List<Pawn> outNonParticipants)
        {
            foreach (var pawn in inPawns)
            {
                if (CanParticipate(pawn, category))
                    outParticipants.Add(pawn);
                else
                    outNonParticipants.Add(pawn);
            }
            outParticipants.SortByDescending(p => TournamentEffectivenessFor(p, category));
        }

        public static string ToStringHuman(this TournamentCategory category)
        {
            return $"VanillaFactionsExpanded.TournamentCategory_{category}".Translate();
        }

        public static Pawn GenerateNewCompetitor(TournamentCategory category, Faction faction)
        {
            return PawnGenerator.GeneratePawn(new PawnGenerationRequest(faction.RandomPawnKind(), faction, forceGenerateNewPawn: true, validatorPreGear: p => CanParticipate(p, category)));
        }

        public static List<Pawn> GenerateCompetitors(int count, TournamentCategory category, Faction faction, IEnumerable<Pawn> existingPawns = null)
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
