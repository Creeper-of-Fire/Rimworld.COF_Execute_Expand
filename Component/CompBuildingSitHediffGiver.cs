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

        public CompProperties_BuildingSitHediffGiver() => compClass = typeof(CompBuildingSitHediffGiver);
    }

    public class CompBuildingSitHediffGiver : ThingComp
    {
        private CompProperties_BuildingSitHediffGiver Props => (CompProperties_BuildingSitHediffGiver)props;

        //private int CompHediffGiverCount ;
        public override void CompTickRare()
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
        }
    }
}