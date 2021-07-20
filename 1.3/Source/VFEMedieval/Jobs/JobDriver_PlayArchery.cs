using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFEMedieval
{
	public class JobDriver_PlayArchery : JobDriver_WatchBuilding
	{
		Thing bow;
		protected override void WatchTickAction()
		{
			if (pawn.IsHashIntervalTick(400))
			{
				ThrowObjectAt(pawn, base.TargetA.Cell, VFEM_DefOf.VFEM_ArrowThrowable);
			}
			base.WatchTickAction();
		}

		public void DrawEquipment(Vector3 rootLoc, Rot4 pawnRotation, PawnRenderFlags flags)
		{
			if (pawn.pather.Moving)
            {
				return;
            }
			Vector3 drawLoc = new Vector3(0f, (pawnRotation == Rot4.North) ? (-0.00289575267f) : 0.03474903f, 0f);
			Vector3 vector = TargetA.Thing.DrawPos;
			float num = 0f;
			if ((vector - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
			{
				num = (vector - pawn.DrawPos).AngleFlat();
			}
			drawLoc += rootLoc + new Vector3(0f, 0f, 0.4f).RotatedBy(num);
			if (bow is null)
			{
				bow = ThingMaker.MakeThing(VFEM_DefOf.Bow_Great);
			}
			DrawEquipmentAiming(bow, drawLoc, num);
		}

		public void DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle)
		{
			Mesh mesh = null;
			float num = aimAngle - 90f;
			if (aimAngle > 20f && aimAngle < 160f)
			{
				mesh = MeshPool.plane10;
				num += eq.def.equippedAngleOffset;
			}
			else if (aimAngle > 200f && aimAngle < 340f)
			{
				mesh = MeshPool.plane10Flip;
				num -= 180f;
				num -= eq.def.equippedAngleOffset;
			}
			else
			{
				mesh = MeshPool.plane10;
				num += eq.def.equippedAngleOffset;
			}
			num %= 360f;
			CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
			if (compEquippable != null)
			{
				EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out var drawOffset, out var angleOffset, aimAngle);
				drawLoc += drawOffset;
				num += angleOffset;
			}
			Material material = null;
			Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
			Graphics.DrawMesh(material: (graphic_StackCount == null) ? eq.Graphic.MatSingle : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle, mesh: mesh, position: drawLoc, rotation: Quaternion.AngleAxis(num, Vector3.up), layer: 0);
		}
		private static void ThrowObjectAt(Pawn thrower, IntVec3 targetCell, FleckDef fleck)
		{
			if (thrower.Position.ShouldSpawnMotesAt(thrower.Map))
			{
				float num = Rand.Range(9.8f, 15.6f);
				Vector3 vector = targetCell.ToVector3Shifted() + Vector3Utility.RandomHorizontalOffset((1f - (float)thrower.skills.GetSkill(SkillDefOf.Shooting).Level / 20f) * 1.8f);
				vector.y = thrower.DrawPos.y;
				FleckCreationData dataStatic = FleckMaker.GetDataStatic(thrower.DrawPos, thrower.Map, fleck);
				dataStatic.rotationRate = 0;
				dataStatic.rotation = thrower.Rotation.AsAngle;
				dataStatic.velocityAngle = (vector - dataStatic.spawnPosition).AngleFlat();
				dataStatic.velocitySpeed = num;
				dataStatic.airTimeLeft = Mathf.RoundToInt((dataStatic.spawnPosition - vector).MagnitudeHorizontal() / num);
				thrower.Map.flecks.CreateFleck(dataStatic);
			}
		}
	}
}