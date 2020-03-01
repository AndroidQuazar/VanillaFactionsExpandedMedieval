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

    public class CaravanArrivalAction_AttendMedievalTournament : CaravanArrivalAction
    {

        public CaravanArrivalAction_AttendMedievalTournament()
        {
        }

        public CaravanArrivalAction_AttendMedievalTournament(MedievalTournament tournament)
        {
            this.tournament = tournament;
        }

        public override string Label => "VanillaFactionsExpanded.AttendMedievalTournament".Translate(tournament.Label);

        public override string ReportString => "VanillaFactionsExpanded.CaravanAttending".Translate(tournament.Label);

        public override void Arrived(Caravan caravan)
        {
            tournament.Notify_CaravanArrived(caravan);
        }

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
        {
            var baseValidation = base.StillValid(caravan, destinationTile);
            if (!baseValidation)
                return baseValidation;
            if (tournament != null && tournament.Tile != destinationTile)
                return false;
            return CanVisit(caravan, tournament);
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref tournament, "tournament");
            base.ExposeData();
        }

        public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, MedievalTournament tournament)
        {
            return tournament != null && tournament.Spawned;
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, MedievalTournament tournament)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(caravan, tournament), () => new CaravanArrivalAction_AttendMedievalTournament(tournament),
                "VanillaFactionsExpanded.AttendMedievalTournament".Translate(tournament.Label), caravan, tournament.Tile, tournament);
        }

        private MedievalTournament tournament;

    }

}
