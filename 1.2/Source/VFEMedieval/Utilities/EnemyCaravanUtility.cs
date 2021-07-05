using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace VFEMedieval
{

    public static class EnemyCaravanUtility
    {

        public static EnemyCaravan MakeCaravan(Faction faction, int startTile, int destinationTile)
        {
            var caravan = (EnemyCaravan)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.VFEM_EnemyCaravan);
            caravan.Tile = startTile;
            caravan.pather.StartPath(destinationTile, null);
            caravan.SetFaction(faction);
            return caravan;
        }

        public static void Attack(Caravan caravan, EnemyCaravan enemyCaravan)
        {
            LongEventHandler.QueueLongEvent(() => AttackNow(caravan, enemyCaravan), "GeneratingMapForNewEncounter", false, null);
        }

        private static void AttackNow(Caravan caravan, EnemyCaravan enemyCaravan)
        {
            Pawn t = caravan.PawnsListForReading[0];
            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(enemyCaravan.Tile, null);
            string letterLabel = "LetterLabelCaravanEnteredEnemyBase".Translate();
            string letterText = "LetterCaravanEnteredEnemyBase".Translate(caravan.Label, enemyCaravan.Label).CapitalizeFirst();
            AffectRelationsOnAttacked(enemyCaravan, ref letterText);
            Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns, ref letterLabel, ref letterText, "LetterRelatedPawnsSettlement".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, t, enemyCaravan.Faction);
            CaravanEnterMapUtility.Enter(caravan, orGenerateMap, CaravanEnterMode.Edge, CaravanDropInventoryMode.DoNotDrop, draftColonists: true);
        }

        public static void AffectRelationsOnAttacked(EnemyCaravan enemyCaravan, ref string letterText)
        {
            if (enemyCaravan.Faction == null || enemyCaravan.Faction == Faction.OfPlayer)
            {
                return;
            }
            FactionRelationKind playerRelationKind = enemyCaravan.Faction.PlayerRelationKind;
            if (!enemyCaravan.Faction.HostileTo(Faction.OfPlayer))
            {
                enemyCaravan.Faction.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Hostile, canSendLetter: false);
            }
            else if (enemyCaravan.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -50, canSendMessage: false, canSendHostilityLetter: false))
            {
                if (!letterText.NullOrEmpty())
                {
                    letterText += "\n\n";
                }
                letterText = letterText + "RelationsWith".Translate(enemyCaravan.Faction.Name) + ": " + (-50).ToStringWithSign();
            }
            enemyCaravan.Faction.TryAppendRelationKindChangedInfo(ref letterText, playerRelationKind, enemyCaravan.Faction.PlayerRelationKind);
        }

    }

}
