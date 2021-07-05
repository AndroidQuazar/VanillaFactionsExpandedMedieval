using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;

namespace VFEMedieval
{

    public class CaravanArrivalAction_AttackEnemyCaravan : CaravanArrivalAction
    {

        public CaravanArrivalAction_AttackEnemyCaravan()
        {
        }

        public CaravanArrivalAction_AttackEnemyCaravan(EnemyCaravan enemyCaravan)
        {
            this.enemyCaravan = enemyCaravan;
        }

        public override string Label => "VanillaFactionsExpanded.AttackEnemyCaravan".Translate(enemyCaravan.Label);

        public override string ReportString => "CaravanAttacking".Translate(enemyCaravan.Label);

        public override void Arrived(Caravan caravan)
        {
            EnemyCaravanUtility.Attack(caravan, enemyCaravan);
        }

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
        {
            var baseValidation = base.StillValid(caravan, destinationTile);
            if (!baseValidation)
                return baseValidation;
            if (enemyCaravan != null && enemyCaravan.Tile != destinationTile)
                caravan.pather.StartPath(enemyCaravan.Tile, this, true);
            return CanVisit(caravan, enemyCaravan);
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref enemyCaravan, "enemyCaravan");
            base.ExposeData();
        }

        public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, EnemyCaravan enemyCaravan)
        {
            return enemyCaravan != null && enemyCaravan.Spawned;
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, EnemyCaravan enemyCaravan)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(caravan, enemyCaravan), () => new CaravanArrivalAction_AttackEnemyCaravan(enemyCaravan),
                "VanillaFactionsExpanded.AttackEnemyCaravan".Translate(enemyCaravan.Label), caravan, enemyCaravan.Tile, enemyCaravan);
        }

        private EnemyCaravan enemyCaravan;

    }

}
