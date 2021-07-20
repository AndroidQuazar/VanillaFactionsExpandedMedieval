using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VFEMedieval
{
    public class JobGiver_AttackOtherHostiles : JobGiver_AIFightEnemies
    {
        public List<Pawn> enemies = new List<Pawn>();
        protected override Thing FindAttackTarget(Pawn pawn)
        {
            TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns
                | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
            if (PrimaryVerbIsIncendiary(pawn))
            {
                targetScanFlags |= TargetScanFlags.NeedNonBurning;
            }
            var target = enemies.RandomElement();
            if (target is null)
            {
                return (Thing)AttackTargetFinder.BestAttackTarget
                (pawn, targetScanFlags, (Thing x) => ExtraTargetValidator(pawn, x), 0f, pawn.Map.Size.x, GetFlagPosition(pawn), GetFlagRadius(pawn));
            }
            return target;
        }

        private bool PrimaryVerbIsIncendiary(Pawn pawn)
        {
            if (pawn.equipment != null && pawn.equipment.Primary != null)
            {
                List<Verb> allVerbs = pawn.equipment.Primary.GetComp<CompEquippable>().AllVerbs;
                for (int i = 0; i < allVerbs.Count; i++)
                {
                    if (allVerbs[i].verbProps.isPrimary)
                    {
                        return allVerbs[i].IsIncendiary();
                    }
                }
            }
            return false;
        }
    }

    public class GenStep_Skirmish : GenStep
	{
		public override int SeedPart => 398638181;
		public override void Generate(Map map, GenStepParams parms)
		{
            Log.Message("map.ParentFaction: " + map.ParentFaction);
            GeneratePawns(map, map.ParentFaction);
        }

        Predicate<Faction> baseValidator = delegate (Faction x)
        {
            if (x.hidden ?? false)
            {
                return false;
            }
            if (x.IsPlayer)
            {
                return false;
            }
            if (x.defeated)
            {
                return false;
            }
            if (!x.def.humanlikeFaction)
            {
                return false;
            }
            if (x.def.pawnGroupMakers == null || !x.def.pawnGroupMakers.Where(y => y.kindDef == PawnGroupKindDefOf.Combat).Any())
            {
                return false;
            }
            return true;
        };

        protected bool GeneratePawns(Map map, Faction friendlyFaction)
        {
            var hostileFactions = Find.FactionManager.AllFactions.Where(x => baseValidator(x) && x.HostileTo(friendlyFaction));
            var enemyFaction = hostileFactions.Where(x => x.HostileTo(friendlyFaction)).RandomElement();

            var points = StorytellerUtility.DefaultThreatPointsNow(Find.World);
            if (points > 10000)
            {
                points = 10000;
            }
            points = Mathf.Max(points, friendlyFaction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));

            var raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            var friendlyParms = new IncidentParms();
            friendlyParms.target = map;
            friendlyParms.faction = friendlyFaction;
            friendlyParms.points = points;
            friendlyParms.raidStrategy = RaidStrategyDefOf.ImmediateAttackFriendly;
            friendlyParms.raidArrivalMode = raidArrivalMode;
            MapGenerator.PlayerStartSpot = IntVec3.Zero;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out friendlyParms.spawnCenter, map, CellFinder.EdgeRoadChance_Friendly))
            {
                return false;
            }
            var enemyParms = new IncidentParms();
            enemyParms.target = map;
            enemyParms.faction = enemyFaction;
            enemyParms.points = points * 1.5f;
            enemyParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            enemyParms.raidArrivalMode = raidArrivalMode;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out enemyParms.spawnCenter, map, CellFinder.EdgeRoadChance_Hostile, allowFogged: true,
                (IntVec3 x) => x.DistanceTo(friendlyParms.spawnCenter) > map.Size.x))
            {
                return false;
            }

            var enemies = SpawnRaid(enemyParms, out List<TargetInfo> targetInfosEnemies);
            if (!enemies.Any() || !targetInfosEnemies.Any())
            {
                return false;
            }
            var friendlies = SpawnRaid(friendlyParms, out List<TargetInfo> targetInfosFriendlies);
            if (!friendlies.Any() || !targetInfosFriendlies.Any())
            {
                return false;
            }
            foreach (var enemy in enemies)
            {
                enemy.mindState.enemyTarget = friendlies.RandomElement();
                var jbg = new JobGiver_AttackOtherHostiles();
                jbg.ResolveReferences();
                jbg.enemies = friendlies;
                var result = jbg.TryIssueJobPackage(enemy, default(JobIssueParams));
                if (result.Job != null)
                {
                    enemy.jobs.TryTakeOrderedJob(result.Job);
                }
            }
            return true;
        }

        private List<Pawn> SpawnRaid(IncidentParms parms, out List<TargetInfo> targetInfos)
        {
            PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;
            parms.raidStrategy.Worker.TryGenerateThreats(parms);
            List<Pawn> list = parms.raidStrategy.Worker.SpawnThreats(parms);
            if (list == null)
            {
                list = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms)).ToList();
                if (list.Count == 0)
                {
                    Log.Error("Got no pawns spawning raid from parms " + parms);
                    targetInfos = null;
                    return list;
                }
                parms.raidArrivalMode.Worker.Arrive(list, parms);
            }

            List<TargetInfo> list2 = new List<TargetInfo>();
            if (parms.pawnGroups != null)
            {
                List<List<Pawn>> list3 = IncidentParmsUtility.SplitIntoGroups(list, parms.pawnGroups);
                List<Pawn> list4 = list3.MaxBy((List<Pawn> x) => x.Count);
                if (list4.Any())
                {
                    list2.Add(list4[0]);
                }
                for (int i = 0; i < list3.Count; i++)
                {
                    if (list3[i] != list4 && list3[i].Any())
                    {
                        list2.Add(list3[i][0]);
                    }
                }
            }
            else if (list.Any())
            {
                foreach (Pawn item in list)
                {
                    list2.Add(item);
                }
            }
            parms.raidStrategy.Worker.MakeLords(parms, list);
            targetInfos = list2;
            return list;
        }
    }
}