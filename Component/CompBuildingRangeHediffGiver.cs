using System.Collections.Generic;
using COF_Torture.Cell;
using COF_Torture.Data;
using COF_Torture.Hediffs;
using COF_Torture.Things;
using Verse;

namespace COF_Torture.Component
{
    public class CompProperties_BuildingRangeHediffGiver : CompProperties
    {
        public HediffDef hediff;
        public BodyPartDef part;
        public float range;

        public CompProperties_BuildingRangeHediffGiver() => compClass = typeof(Comp_BuildingRangeHediffGiver);
    }

    public class Comp_BuildingRangeHediffGiver : ThingComp
    {
        private CompProperties_BuildingRangeHediffGiver Props => (CompProperties_BuildingRangeHediffGiver)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            var effector = new CellEffector_AddHediffWhenPass(this.Props.hediff, this.Props.part);
            
            var parentMap = this.parent.Map;
            if (parentMap == null) return;
            var data = DataUtility.GetCellEffectorData(parentMap);
            if (data == null) return;
            
            var posList = new List<IntVec3>();
            for (var index1 = 0; index1 < GenRadial.NumCellsInRadius(this.Props.range); ++index1)
            {
                posList.Add(this.parent.Position + GenRadial.RadialPattern[index1]);
            }
            
            foreach (var pos in posList)
            {
                data.AddCellEffector(effector, this.parent, pos);
            }
        }

        //private int CompHediffGiverCount ;
        /*public override void CompTickRare()
        {
            base.CompTickRare();
            List<Pawn> allPawnsSpawned = parent.Map.mapPawns.AllPawnsSpawned;
            foreach (var t in allPawnsSpawned)
            {
                if (t.Position.Equals(parent.Position) && t.jobs != null && t.pather != null && !t.pather.Moving)
                {
                    var a = t.health.hediffSet.GetNotMissingParts()
                        .FirstOrFallback(
                            p => p.def == Props.part);
                    Hediff_WithGiver h = (Hediff_WithGiver)HediffMaker.MakeHediff(Props.hediff, t, a);
                    h.Giver = (Building_TortureBed)parent;
                    t.health.AddHediff(h);
                }
                else
                {
                    var h = (Hediff_WithGiver)t.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                    if (h != null && h.Giver == parent)
                    {
                        t.health.RemoveHediff(h);
                    }
                }
            }
        }*/
    }
}