using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;

namespace VFEMedieval
{
    class Designator_FillTerrain : Designator
    {
		public override int DraggableDimensions
		{
			get
			{
				return 2;
			}
		}

		public override bool DragDrawMeasurements
		{
			get
			{
				return true;
			}
		}

		public Designator_FillTerrain()
		{
			this.defaultLabel = "DesignatorFillTerrain".Translate();
			this.defaultDesc = "DesignatorFillTerrainDesc".Translate();
			this.icon = ContentFinder<Texture2D>.Get("Designations/FillTerrain", true);
			this.useMouseIcon = true;
			this.soundDragSustain = SoundDefOf.Designate_DragStandard;
			this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			this.soundSucceeded = SoundDefOf.Designate_SmoothSurface;
			this.hotKey = KeyBindingDefOf.Misc12;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (c.Fogged(base.Map))
			{
				return false;
			}
			if (base.Map.designationManager.DesignationAt(c, VFEM_DefOf.VFEM_FillTerrain) != null || base.Map.designationManager.DesignationAt(c, VFEM_DefOf.VFEM_FillTerrain) != null)
			{
				return "TerrainBeingFilled".Translate();
			}
			if (c.InNoBuildEdgeArea(base.Map))
			{
				return "TooCloseToMapEdge".Translate();
			}
			if (!DefDatabase<TerrainListDef>.GetNamed("VFEM_MoatableTerrain").terrainDefs.Contains(c.GetTerrain(base.Map)))
			{
				return "MessageMustDesignateMoatableTerrain".Translate();
			}
			if (c.GetTerrain(base.Map) == TerrainDef.Named("Soil"))
			{
				return "AtHighestLevel".Translate();
			}
			if (c.GetTerrain(base.Map) == TerrainDef.Named("WaterShallow") || c.GetTerrain(base.Map) == TerrainDef.Named("Marsh"))
			{
				return "CantFillWater".Translate();
			}
			return AcceptanceReport.WasAccepted;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			Building edifice = c.GetEdifice(base.Map);
			if (edifice != null && edifice.def.IsSmoothable)
			{
				base.Map.designationManager.AddDesignation(new Designation(c, VFEM_DefOf.VFEM_FillTerrain));
				return;
			}
			base.Map.designationManager.AddDesignation(new Designation(c, VFEM_DefOf.VFEM_FillTerrain));
		}

		public override void SelectedUpdate()
		{
			GenUI.RenderMouseoverBracket();
		}

		public override void RenderHighlight(List<IntVec3> dragCells)
		{
			DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
		}
	}
}
