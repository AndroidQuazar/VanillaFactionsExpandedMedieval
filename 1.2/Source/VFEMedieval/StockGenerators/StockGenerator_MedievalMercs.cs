using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFEMedieval
{

    public class StockGenerator_MedievalMercs : StockGenerator
    {

        private const float CosplayChance = 0.01f;

        public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction=null)
        {
            if (respectPopulationIntent && Rand.Value > StorytellerUtilityPopulation.PopulationIntent)
                yield break;

            int count = countRange.RandomInRange;
            for (int i = 0; i < count; i++)
            {
                if (!Find.FactionManager.AllFactionsVisible.Where(f => f.def.techLevel == TechLevel.Medieval && f != Faction.OfPlayer && f.def.humanlikeFaction).TryRandomElement(out Faction mercFaction))
                    yield break;

                PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.VFEM_SellSword, mercFaction, PawnGenerationContext.NonPlayer, forTile, mustBeCapableOfViolence: true);
                var merc = PawnGenerator.GeneratePawn(request);

                // Geralt!
                if (Rand.Chance(CosplayChance))
                {
                    // Change name
                    var name = (NameTriple)merc.Name;
                    merc.Name = new NameTriple(name.First, "Geralt", name.Last);

                    // Make the hair white
                    merc.story.hairColor = Color.white;

                    // Replace equipment with a silver sword
                    merc.equipment.DestroyAllEquipment();
                    var silverSword = (ThingWithComps)ThingMaker.MakeThing(ThingDefOf.MeleeWeapon_LongSword, RimWorld.ThingDefOf.Silver);
                    merc.equipment.AddEquipment(silverSword);

                    // Remove headgear
                    for (int j = 0; j < merc.apparel.WornApparelCount; j++)
                    {
                        var apparel = merc.apparel.WornApparel[j];
                        if (apparel.def.apparel.layers.Contains(RimWorld.ApparelLayerDefOf.Overhead))
                            merc.apparel.Remove(apparel);
                    }
                }

                yield return merc;
            }
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return thingDef.category == ThingCategory.Pawn && thingDef.race.Humanlike && thingDef.tradeability != Tradeability.None;
        }

        private bool respectPopulationIntent;
    }

}
