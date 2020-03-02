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
using VFECore;

namespace VFEMedieval
{

    public class GenStep_CastleRuins : GenStep_Scatterer
    {

        private static readonly IntRange SettlementSizeRange = new IntRange(34, 38);

        public override int SeedPart => 87582;

        protected override bool CanScatterAt(IntVec3 c, Map map)
        {
            if (!base.CanScatterAt(c, map))
            {
                return false;
            }
            if (!c.Standable(map))
            {
                return false;
            }
            if (c.Roofed(map))
            {
                return false;
            }
            if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors)))
            {
                return false;
            }
            IntRange settlementSizeRange = SettlementSizeRange;
            int min = settlementSizeRange.min;
            CellRect cellRect = new CellRect(c.x - min / 2, c.z - min / 2, min, min);
            IntVec3 size = map.Size;
            int x = size.x;
            IntVec3 size2 = map.Size;
            if (!cellRect.FullyContainedWithin(new CellRect(0, 0, x, size2.z)))
            {
                return false;
            }
            return true;
        }

        protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            int randomInRange = SettlementSizeRange.RandomInRange;
            int randomInRange2 = SettlementSizeRange.RandomInRange;
            CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
            rect.ClipInsideMap(map);
            var faction = ExtendedFactionUtility.RandomFactionOfTechLevel(TechLevel.Medieval, allowDefeated: true);
            ResolveParams resolveParams = default;
            resolveParams.faction = faction;
            resolveParams.rect = rect;
            resolveParams.noRoof = true;
            resolveParams.chanceToSkipWallBlock = 0.22f;
            resolveParams.SetCustom(VFEResolveParams.Name, new VFEResolveParams());
            var vfeParams = resolveParams.GetCustom<VFEResolveParams>(VFEResolveParams.Name);
            Log.Message(vfeParams.ToStringSafe());
            vfeParams.hasDoors = false;
            vfeParams.outdoorLighting = false;
            vfeParams.generatePawns = false;
            BaseGen.globalSettings.map = map;
            BaseGen.globalSettings.minBuildings = 1;
            BaseGen.globalSettings.minBarracks = 1;
            BaseGen.symbolStack.Push("VFEM_medievalSettlement", resolveParams);
            BaseGen.Generate();
        }

    }

}
