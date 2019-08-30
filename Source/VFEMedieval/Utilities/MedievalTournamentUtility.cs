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
        public static readonly IntRange CompetitorCountRange = new IntRange(4, 9);

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
            return Mathf.Pow(accuracy, 3.6f) * MentalBreakThresholdMultiplierFor(pawn, 0.25f) * MeleeEffectivenessNormaliser;
        }

        private static float BaseJoustEffectivenessFor(Pawn pawn)
        {
            float accuracy = pawn.GetStatValue(StatDefOf.MeleeHitChance);
            float tameChance = pawn.GetStatValue(StatDefOf.TameAnimalChance);
            return (Mathf.Pow(accuracy, 3.6f) * JoustEffectivenessNormaliser + Mathf.Pow(tameChance, 0.625f) * JoustEffectivenessNormaliser) * MentalBreakThresholdMultiplierFor(pawn, 0.25f);
        }

        private static float BaseArcheryEffectivenessFor(Pawn pawn)
        {
            float accuracy = ShotReport.HitFactorFromShooter(pawn, archeryDistance);
            return accuracy * MentalBreakThresholdMultiplierFor(pawn, 0.5f) * ArcheryEffectivenessNormaliser;
        }

        public static float TournamentEffectivenessFor(Pawn pawn, TournamentCategory category)
        {
            float baseEffectiveness = 0;
            switch(category)
            {
                case TournamentCategory.Melee:
                    baseEffectiveness = BaseMeleeEffectivenessFor(pawn);
                    break;
                case TournamentCategory.Jousting:
                    baseEffectiveness = BaseJoustEffectivenessFor(pawn);
                    break;
                case TournamentCategory.Archery:
                    baseEffectiveness = BaseArcheryEffectivenessFor(pawn);
                    break;
                default:
                    throw new NotImplementedException();
            }
            return Mathf.Pow(baseEffectiveness, 2);
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
            if (effectiveness < 5)
                resultStr = "QualityCategory_Awful".Translate();
            else if (effectiveness < 20)
                resultStr = "QualityCategory_Poor".Translate();
            else if (effectiveness < 40)
                resultStr = "Average".Translate();
            else if (effectiveness < 70)
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

    }

}
