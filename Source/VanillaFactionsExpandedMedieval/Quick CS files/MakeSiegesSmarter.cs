using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using Harmony;

namespace VFEMedieval
{

    public static class MakeSiegesSmarter
    {

        [HarmonyPatch(typeof(JobDriver_ManTurret), nameof(JobDriver_ManTurret.FindAmmoForTurret))]
        public static class Patch_FindAmmoForTurret
        {

            public static bool Prefix(Pawn pawn, Building_TurretGun gun, ref Thing __result)
            {
                // Destructive detour on this method to make raiders less dumb when seeking shells for their death machines
                // Method is small enough that this shouldn't be a big deal
                var allowedShellsSettings = gun.gun.TryGetComp<CompChangeableProjectile>().allowedShellsSettings;
                Predicate<Thing> validator = (Thing t) => !t.IsForbidden(pawn) && pawn.CanReserve(t, 10, 1, null, false) && allowedShellsSettings.AllowedToAccept(t);
                __result = GenClosest.ClosestThingReachable(gun.Position, gun.Map, ThingRequest.ForGroup(ThingRequestGroup.Shell), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 40f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
                return false;
            }

        }

        [HarmonyPatch(typeof(LordToil_Siege), nameof(LordToil_Siege.LordToilTick))]
        public static class Patch_LordToilSiegeTick
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var getIsShellInfo = AccessTools.Property(typeof(ThingDef), nameof(ThingDef.IsShell)).GetGetMethod();
                var getItemInfo = AccessTools.Method(typeof(List<Thing>), "get_Item");

                var isActuallyValidShellInfo = AccessTools.Method(typeof(Patch_LordToilSiegeTick), nameof(IsActuallyValidShell));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Look for any calls to IsShell (should only be one); we need to make this check SMARTER!
                    if (instruction.opcode == OpCodes.Callvirt && instruction.operand == getIsShellInfo)
                    {
                        yield return instruction; // thingList[j].def.IsShell
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 6); // thingList
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 7); // j
                        yield return new CodeInstruction(OpCodes.Callvirt, getItemInfo); // thingList[j]
                        yield return new CodeInstruction(OpCodes.Ldloc_0); // data
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                        instruction = new CodeInstruction(OpCodes.Call, isActuallyValidShellInfo); // IsActuallyValidShell(thingList[j].def.IsShell, thingList[j], data, this)
                    }

                    yield return instruction;
                }
            }

            private static bool IsActuallyValidShell(bool isShell, Thing thing, LordToilData_Siege data, LordToil_Siege instance)
            {
                var raiderFaction = instance.lord.faction;
                var listerThings = instance.Map.listerThings;
                var validArtillery = listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Where(b => b.Faction == raiderFaction && TurretGunUtility.NeedsShells(b.def));
                var validFrameAndBlueprintEntities = ((IEnumerable<Thing>)listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame)).Concat(listerThings.ThingsInGroup(ThingRequestGroup.Blueprint)).
                    Where(f => f.Faction == raiderFaction).Select(f => f.def.entityDefToBuild).Where(e => e is ThingDef t && TurretGunUtility.NeedsShells(t)).Cast<ThingDef>();

                return isShell &&
                    (validArtillery.Any(a => ((Building_TurretGun)a).gun.TryGetComp<CompChangeableProjectile>().allowedShellsSettings.AllowedToAccept(thing)) ||
                    validFrameAndBlueprintEntities.Any(t => t.building.turretGunDef.building.defaultStorageSettings.AllowedToAccept(thing)));
            }

        }

        [HarmonyPatch(typeof(LordToil_Siege), "SetAsBuilder")]
        public static class Patch_LordToilSiegeSetAsBuilder
        {

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();

                var actualMinimumConstructionSkillInfo = AccessTools.Method(typeof(Patch_LordToilSiegeSetAsBuilder), nameof(ActualMinimumConstructionSkill));

                for (int i = 0; i < instructionList.Count; i++)
                {
                    var instruction = instructionList[i];

                    // Replace minLevel with the actual minimum level for blueprinted artillery if appropriate
                    if (instruction.opcode == OpCodes.Ldloc_1)
                    {
                        yield return instruction; // minLevel
                        yield return new CodeInstruction(OpCodes.Ldloc_0); // data
                        yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                        instruction = new CodeInstruction(OpCodes.Call, actualMinimumConstructionSkillInfo); // ActualMinimumConstructionSkill(minLevel, data, this)
                    }

                    yield return instruction;
                }
            }

            private static int ActualMinimumConstructionSkill(int minLevel, LordToilData_Siege data, LordToil_Siege instance)
            {
                var blueprintedBuildings = instance.Map.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint).
                    Where(p => p.Faction == instance.lord.faction && p.Position.InHorDistOf(data.siegeCenter, data.baseRadius)).Select(p => p.def.entityDefToBuild).Where(e => e is ThingDef).Cast<ThingDef>();

                if (blueprintedBuildings.Any())
                    return Mathf.Max(minLevel, blueprintedBuildings.Select(b => b.constructionSkillPrerequisite).Max());
                return minLevel;
            }

        }

    }

}
