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
using Harmony;

namespace VFEMedieval
{

    public class SymbolResolver_AddMustToWineBarrels : SymbolResolver
    {

        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            wineFermenters.Clear();
            CellRect.CellRectIterator iterator = rp.rect.GetIterator();
            while (!iterator.Done())
            {
                List<Thing> thingList = iterator.Current.GetThingList(map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    var wineFermenter = thingList[i].TryGetComp<CompWineFermenter>();
                    if (wineFermenter != null && !wineFermenters.Contains(wineFermenter))
                    {
                        wineFermenters.Add(wineFermenter);
                    }
                }
                iterator.MoveNext();
            }
            float progress = Rand.Range(0.1f, 0.9f);
            for (int j = 0; j < wineFermenters.Count; j++)
            {
                wineFermenters[j].targetQuality = QualityUtility.GenerateQualityBaseGen();
                if (!wineFermenters[j].Fermented)
                {
                    int num = Rand.RangeInclusive(1, 25);
                    num = Mathf.Min(num, wineFermenters[j].SpaceLeftForMust);
                    if (num > 0)
                    {
                        wineFermenters[j].AddMust(num);
                        wineFermenters[j].Progress = progress;
                    }
                }
            }
            wineFermenters.Clear();
        }

        private static List<CompWineFermenter> wineFermenters = new List<CompWineFermenter>();
    }

}
