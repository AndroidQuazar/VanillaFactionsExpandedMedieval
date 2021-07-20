using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace VFEMedieval
{
    class WorkGiver_ConstructFillTerrain : WorkGiver_ConstructAffectFloor
    {
		protected override DesignationDef DesDef
		{
			get
			{
				return VFEM_DefOf.VFEM_FillTerrain;
			}
		}

		public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
		{
			return JobMaker.MakeJob(VFEM_JobDefOf.VFEM_FillTerrain, c);
		}
	}
}
