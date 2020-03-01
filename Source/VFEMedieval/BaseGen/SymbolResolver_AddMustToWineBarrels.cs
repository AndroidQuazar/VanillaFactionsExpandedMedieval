using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using RimWorld.BaseGen;
using HarmonyLib;

namespace VFEMedieval
{

    public class SymbolResolver_AddMustToWineBarrels : SymbolResolver
    {

        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            wineFermenters.Clear();

            foreach (var pos in rp.rect)
            {
                var thingList = pos.GetThingList(map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    var wineFermenterComp = thingList[i].TryGetComp<CompWineFermenter>();
                    if (wineFermenterComp != null && !wineFermenters.Contains(wineFermenterComp))
                        wineFermenters.Add(wineFermenterComp);
                }
            }

            float legendaryAgeTicksFactor = Rand.Range(0.1f, 1.1f);
            for (int i = 0; i < wineFermenters.Count; i++)
            {
                var fermenter = wineFermenters[i];
                fermenter.targetQuality = (QualityCategory)Mathf.Min((int)QualityUtility.GenerateQualityBaseGen() + 1, (int)QualityCategory.Legendary);
                if (!fermenter.Fermented)
                {
                    int mustToAdd = Mathf.Min(Rand.Range(1, fermenter.Props.mustCapacity), fermenter.SpaceLeftForMust);
                    if (mustToAdd > 0)
                    {
                        fermenter.AddMust(mustToAdd);
                        int ageTicks = Mathf.RoundToInt((fermenter.Props.legendaryQualityAgeDaysThreshold * GenDate.TicksPerDay * legendaryAgeTicksFactor) % fermenter.TicksToReachTargetQuality);
                        fermenter.AgeTicks = ageTicks;
                    }
                }
            }

            wineFermenters.Clear();
        }

        private static List<CompWineFermenter> wineFermenters = new List<CompWineFermenter>();
    }

}
