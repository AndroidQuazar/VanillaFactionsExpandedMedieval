using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;

namespace VFEMedieval
{

    public class EnemyCaravan : Caravan
    {

        public void Notify_FactionRelationsChanged()
        {
            if (Faction.PlayerRelationKind != FactionRelationKind.Hostile)
                Find.WorldObjects.Remove(this);
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (var o in base.GetFloatMenuOptions(caravan))
                yield return o;

            foreach (var o in CaravanArrivalAction_AttackEnemyCaravan.GetFloatMenuOptions(caravan, this))
                yield return o;
        }

    }

}
