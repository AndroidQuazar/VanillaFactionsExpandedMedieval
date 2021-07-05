using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace VFEMedieval
{
    public class IncidentWorker_BlackKnight : IncidentWorker_Ambush
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            // Only fire if the target's tile has a road which goes over creek
            var tile = Find.WorldGrid[parms.target.Tile];
            return base.CanFireNowSub(parms) && !tile.Roads.NullOrEmpty() && tile.Rivers?.First().river == RiverDefOf.Creek;
        }

        protected override LordJob CreateLordJob(List<Pawn> generatedPawns, IncidentParms parms)
        {
            // Pawns will defend the point determined in PostProcessGeneratedPawnsAfterSpawning
            return new LordJob_DefendPoint(point);
        }

        protected override List<Pawn> GeneratePawns(IncidentParms parms)
        {
            var pawnList = new List<Pawn>();

            // Spawn the legend himself
            var blackKnight = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.VFEM_BlackKnight, Find.FactionManager.FirstFactionOfDef(PawnKindDefOf.VFEM_BlackKnight.defaultFactionType), mustBeCapableOfViolence: true, fixedGender: Gender.Male));
            pawnList.Add(blackKnight);

            return pawnList;
        }

        protected override void PostProcessGeneratedPawnsAfterSpawning(List<Pawn> generatedPawns)
        {
            // Move all pawns to the point to defend
            var map = generatedPawns.First().Map;
            point = map.AllCells.Where(c => map.terrainGrid.TerrainAt(c) == TerrainDefOf.Bridge).RandomElementByWeight(c => map.Size.x - c.DistanceTo(map.Center));
            for (int i = 0; i < generatedPawns.Count; i++)
            {
                var pawn = generatedPawns[i];
                pawn.SetPositionDirect(point.RandomAdjacentCell8Way());
                pawn.Notify_Teleported(false);
            }
        }

        protected override string GetLetterText(Pawn anyPawn, IncidentParms parms)
        {
            var caravan = parms.target as Caravan;
            return String.Format(def.letterText, caravan?.Name ?? "yourCaravan".Translate()).CapitalizeFirst();
        }

        private IntVec3 point;
    }
}