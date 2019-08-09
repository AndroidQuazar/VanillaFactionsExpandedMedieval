using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using Harmony;

namespace VanillaFactionsExpandedMedieval
{

    public static class Patch_FloatMenuMakerMap
    {

        [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
        public static class AddHumanlikeOrders
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                // It's amazing what it takes just to get a local reference
                var equipmentInfo = typeof(FloatMenuMakerMap).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).
                    First(t => t.Name.Contains("AddHumanlikeOrders") && t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Any(f => f.FieldType == typeof(ThingWithComps) && f.Name == "equipment")).
                    GetField("equipment", BindingFlags.NonPublic | BindingFlags.Instance);

                var addInfo = AccessTools.Method(typeof(List<FloatMenuOption>), "Add");
                var addShieldFloatMenuOptionInfo = AccessTools.Method(typeof(AddHumanlikeOrders), nameof(AddShieldFloatMenuOption));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Look for the section that gives the 'Equip x' float menu instruction; add our 'equip x as shield' float menu method after
                    if (instruction.opcode == OpCodes.Callvirt && instruction.operand == addInfo)
                    {
                        var prevInstruction = instructionList[i - 1];
                        if (prevInstruction.opcode == OpCodes.Ldloc_S && prevInstruction.operand is LocalBuilder lb && lb.LocalIndex == 42)
                        {
                            yield return instruction;
                            yield return new CodeInstruction(OpCodes.Ldarg_1); // pawn
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 39); // equipment
                            yield return new CodeInstruction(OpCodes.Ldfld, equipmentInfo); // Necessary since the 'equipment' variable is a reference to this
                            yield return new CodeInstruction(OpCodes.Ldarga_S, 2); // ref opts
                            instruction = new CodeInstruction(OpCodes.Call, addShieldFloatMenuOptionInfo); // AddShieldFloatMenuOption(pawn, equipment, ref opts)
                        }
                    }

                    yield return instruction;
                }
            }

            private static void AddShieldFloatMenuOption(Pawn pawn, Thing equipment, ref List<FloatMenuOption> opts)
            {
                // Add an extra option to the float menu if the thing is a shield
                if (equipment.IsShield(out CompShield shieldComp))
                {
                    string labelShort = equipment.LabelShort;
                    FloatMenuOption shieldOption;

                    // Pawn is pacifist
                    if (equipment.def.IsWeapon && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                        shieldOption = new FloatMenuOption("CannotEquip".Translate(labelShort) + " (" + "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn) + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);

                    // Pawn cannot path to shield
                    else if (!pawn.CanReach(equipment, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                        shieldOption = new FloatMenuOption("CannotEquip".Translate(labelShort) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);

                    // Pawn cannot manipulate
                    else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !pawn.CanUseShields())
                        shieldOption = new FloatMenuOption("CannotEquip".Translate(labelShort) + " (" + "Incapable".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);

                    // Shield is burning
                    else if (equipment.IsBurning())
                        shieldOption = new FloatMenuOption("CannotEquip".Translate(labelShort) + " (" + "BurningLower".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);

                    // Able to equip shield
                    else
                    {
                        string optionLabel = "VanillaFactionsExpandedMedieval.EquipShield".Translate(labelShort);

                        // I seriously doubt this'll ever return true but hey, why not
                        if (equipment.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
                            optionLabel = optionLabel + " " + "EquipWarningBrawler".Translate();

                        // Primary cannot be used with shields
                        if (pawn.equipment.Primary is ThingWithComps weapon)
                        {
                            var thingDefExtension = weapon.def.GetModExtension<ThingDefExtension>() ?? ThingDefExtension.defaultValues;
                            if (!thingDefExtension.usableWithShields)
                                optionLabel += $" {"VanillaFactionsExpandedMedieval.EquipWarningShieldUnusable".Translate(weapon.def.label)}";
                        }

                        shieldOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(optionLabel, delegate ()
                        {
                            equipment.SetForbidden(false, true);
                            pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.EquipShield, equipment), JobTag.Misc);
                            MoteMaker.MakeStaticMote(equipment.DrawPos, equipment.Map, RimWorld.ThingDefOf.Mote_FeedbackEquip, 1f);
                            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
                        }, MenuOptionPriority.High, null, null, 0f, null, null), pawn, equipment, "ReservedBy");
                    }
                    opts.Add(shieldOption);
                }
            }

        }

    }

}
