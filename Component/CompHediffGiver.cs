using System;
using System.Collections.Generic;
using COF_Torture.Hediffs;
using COF_Torture.Things;
using Verse;

namespace COF_Torture.Component
{
    public class CompProperties_HediffGiver : CompProperties
    {
        public HediffDef hediff;
        public BodyPartDef part;

        public CompProperties_HediffGiver() => this.compClass = typeof(CompHediffGiver);
    }

    public class CompHediffGiver : ThingComp
    {
        private CompProperties_HediffGiver Props => (CompProperties_HediffGiver)this.props;

        //private int CompHediffGiverCount ;

        public override void CompTickRare()
        {
            base.CompTickRare();
            //this.CompHediffGiverCount++;
            // if (this.CompHediffGiverCount > 120)
            //{
            //this.CompHediffGiverCount = 0;
            List<Pawn> allPawnsSpawned = this.parent.Map.mapPawns.AllPawnsSpawned;
            //Log.Message("1");
            foreach (var t in allPawnsSpawned)
            {
                if (t.Position.Equals(this.parent.Position) && t.jobs != null)
                {
                    //Log.Message("2");
                    var a = t.health.hediffSet.GetNotMissingParts()
                        .FirstOrFallback<BodyPartRecord>(
                            (Func<BodyPartRecord, bool>)(p => p.def == this.Props.part));
                    Hediff_WithGiver h = (Hediff_WithGiver)HediffMaker.MakeHediff(this.Props.hediff, t, a);
                    h.giver = (Building_TortureBed)this.parent;
                    t.health.AddHediff(h);
                }
                else
                {
                    var h = (Hediff_WithGiver)t.health.hediffSet.GetFirstHediffOfDef(this.Props.hediff);
                    if (h != null && h.giver == this.parent)
                    {
                        t.health.RemoveHediff(h);
                    }
                }
            }
            //}
        }
    }
}