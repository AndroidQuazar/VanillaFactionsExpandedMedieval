using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.Planet;
using Verse.AI.Group;

namespace VFEMedieval
{

    public class IncidentWorker_QuestCastleRuins : IncidentWorker
    {

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            int tile;
            Faction faction;
            return Find.FactionManager.RandomNonHostileFaction(allowNonHumanlike: false) != null && TryFindTile(out tile) && SiteMakerHelper.TryFindRandomFactionFor(SiteCoreDefOf.VFEM_CastleRuins, null, out faction);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var faction = parms.faction ?? Find.FactionManager.RandomNonHostileFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: false);

            if (faction == null)
                return false;

            if (!TryFindTile(out int tile))
                return false;

            if (!SiteMakerHelper.TryFindSiteParams_SingleSitePart(SiteCoreDefOf.VFEM_CastleRuins, (!Rand.Chance(SiteTuning.ItemStashNoSitePartChance)) ? "VFEM_CastleRuinsQuestThreat" : null, out var sitePart, out var otherFac))
                return false;

            int days = SiteTuning.QuestSiteTimeoutDaysRange.RandomInRange;
            var site = CreateSite(tile, sitePart, days, otherFac);
            string letterText = def.letterText.Formatted(faction.def.leaderTitle, faction.Name, SitePartUtility.GetDescriptionDialogue(site, site.parts.FirstOrDefault()), days, faction.leader.Named("PAWN"));
            Find.LetterStack.ReceiveLetter(def.letterLabel, letterText, def.letterDef, site, faction);
            return true;
        }

        private bool TryFindTile(out int tile)
        {
            IntRange itemStashQuestSiteDistanceRange = SiteTuning.ItemStashQuestSiteDistanceRange;
            return TileFinder.TryFindNewSiteTile(out tile, itemStashQuestSiteDistanceRange.min, itemStashQuestSiteDistanceRange.max);
        }

        public static Site CreateSite(int tile, SitePartDef sitePart, int days, Faction siteFaction)
        {
            Site site = SiteMaker.MakeSite(SiteCoreDefOf.VFEM_CastleRuins, sitePart, tile, siteFaction);
            site.GetComponent<TimeoutComp>().StartTimeout(days * 60000);
            Find.WorldObjects.Add(site);
            return site;
        }

    }

}
