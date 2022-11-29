using System;
using System.Collections.Generic;
using COF_Torture.Hediffs;
using COF_Torture.Things;
using Verse;

namespace COF_Torture.Component
{
    public class CompProperties_BuildingSitHediffGiver : CompProperties
    {
        public HediffDef hediff;
        public BodyPartDef part;

        public CompProperties_BuildingSitHediffGiver() => this.compClass = typeof(CompBuildingSitHediffGiver);
    }

    public class CompBuildingSitHediffGiver : ThingComp
    {
        private CompProperties_BuildingSitHediffGiver Props => (CompProperties_BuildingSitHediffGiver)this.props;

        //private int CompHediffGiverCount ;
        public override void CompTickRare()
        {
            base.CompTickRare();
            List<Pawn> allPawnsSpawned = this.parent.Map.mapPawns.AllPawnsSpawned;
            foreach (var t in allPawnsSpawned)
            {
                if (t.Position.Equals(this.parent.Position) && t.jobs != null && t.pather != null && !t.pather.Moving)
                {
                    var a = t.health.hediffSet.GetNotMissingParts()
                        .FirstOrFallback(
                            (Func<BodyPartRecord, bool>)(p => p.def == this.Props.part));
                    Hediff_WithGiver h = (Hediff_WithGiver)HediffMaker.MakeHediff(this.Props.hediff, t, a);
                    h.Giver = (Building_TortureBed)this.parent;
                    t.health.AddHediff(h);
                }
                else
                {
                    var h = (Hediff_WithGiver)t.health.hediffSet.GetFirstHediffOfDef(this.Props.hediff);
                    if (h != null && h.Giver == this.parent)
                    {
                        t.health.RemoveHediff(h);
                    }
                }
            }
        }
    }
}