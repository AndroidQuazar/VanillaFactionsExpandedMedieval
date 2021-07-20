using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace VFEMedieval
{
    public class JobDriver_FillTerrain : JobDriver_AffectFloor
    {
		protected override int BaseWorkAmount
		{
			get
			{
				return 7000;
			}
		}

		protected override DesignationDef DesDef
		{
			get
			{
				return VFEM_DefOf.VFEM_FillTerrain;
			}
		}

		protected override StatDef SpeedStat
		{
			get
			{
				return StatDefOf.GeneralLaborSpeed;
			}
		}

		public JobDriver_FillTerrain()
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
				case "SoftSand":
					newTerrain = TerrainDef.Named("Soil");
					break;
				case "Sand":
					newTerrain = TerrainDef.Named("SoftSand");
					break;
				case "Gravel":
					newTerrain = TerrainDef.Named("Sand");
					break;
				case "MossyTerrain":
					newTerrain = TerrainDef.Named("Gravel");
					break;
				case "MarshyTerrain":
					newTerrain = TerrainDef.Named("MossyTerrain");
					break;
			}

			base.Map.terrainGrid.SetTerrain(base.TargetLocA, newTerrain);
			FilthMaker.RemoveAllFilth(base.TargetLocA, base.Map);
		}
	}
}
