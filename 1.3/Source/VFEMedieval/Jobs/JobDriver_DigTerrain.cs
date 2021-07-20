using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class JobDriver_DigTerrain : JobDriver_AffectFloor
    {
		protected override int BaseWorkAmount
		{
			get
			{
				return 5000;
			}
		}

		protected override DesignationDef DesDef
		{
			get
			{
				return VFEM_DefOf.VFEM_DigTerrain;
			}
		}

		protected override StatDef SpeedStat
		{
			get
			{
				return StatDefOf.GeneralLaborSpeed;
			}
		}

		public JobDriver_DigTerrain()
		{
			this.clearSnow = true;
		}

		protected override void DoEffect(IntVec3 c)
		{
			TerrainDef localTerrain = base.TargetLocA.GetTerrain(base.Map);
			TerrainDef newTerrain = null;

			switch (localTerrain.ToString())
			{
				case null:
					Log.Error("No terrain found!");
					break;
				case "Soil":
					newTerrain = TerrainDef.Named("SoftSand");
					break;
				case "SoftSand":
					newTerrain = TerrainDef.Named("Sand");
					break;
				case "Sand":
					newTerrain = TerrainDef.Named("Gravel");
					break;
				case "Gravel":
					newTerrain = TerrainDef.Named("MossyTerrain");
					break;
				case "MossyTerrain":
					newTerrain = TerrainDef.Named("MarshyTerrain");
					break;
				case "MarshyTerrain":
					newTerrain = TerrainDef.Named("Marsh");
					break;
				case "Marsh":
					newTerrain = TerrainDef.Named("WaterShallow");
					break;
			}

			base.Map.terrainGrid.SetTerrain(base.TargetLocA, newTerrain);
			FilthMaker.RemoveAllFilth(base.TargetLocA, base.Map);
		}
	}
}
