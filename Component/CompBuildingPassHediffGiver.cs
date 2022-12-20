using System.Collections.Generic;
using COF_Torture.Cell;
using COF_Torture.Data;
using COF_Torture.Hediffs;
using COF_Torture.Things;
using Verse;

namespace COF_Torture.Component
{
    public class CompProperties_BuildingPassHediffGiver : CompProperties
    {
        public HediffDef hediff;
        public BodyPartDef part;

        public CompProperties_BuildingPassHediffGiver() => compClass = typeof(CompBuildingPassHediffGiver);
    }

    public class CompBuildingPassHediffGiver : ThingComp
    {
        private CompProperties_BuildingPassHediffGiver Props => (CompProperties_BuildingPassHediffGiver)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            var effector = new CellEffector_AddHediffWhenPass(this.Props.hediff,this.Props.part);
            var parentMap = this.parent.Map;
            if (parentMap != null)
            {
                var data = DataUtility.GetCellEffectorData(parentMap);
                data?.AddCellEffector(effector,this.parent);
            }
        }
    }
}