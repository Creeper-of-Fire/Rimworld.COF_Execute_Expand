using COF_Torture.Hediffs;
using COF_Torture.Things;
using COF_Torture.Utility.DefOf;
using Verse;

namespace COF_Torture.Component
{
    public class CompProperties_EffectForBondage : CompProperties
    {
        public HediffDef hediff;
        public BodyPartDef part;

        public CompProperties_EffectForBondage() => compClass = typeof(CompEffectForBondage);
    }

    public class CompEffectForBondage : ThingComp
    {
        public CompProperties_EffectForBondage Props => (CompProperties_EffectForBondage)props;
        public Building_TortureBed Parent => (Building_TortureBed)parent;
        public Pawn Victim => Parent.victim;

        /*public void RemoveEffect()
        {
            var t = this.Victim.health;
            Hediff_WithGiver h = t.hediffSet.GetFirstHediffOfDef(this.Props.hediff) as Hediff_WithGiver;
            if (h != null)
            {
                h.giver = null;
                t.RemoveHediff(h);
            }

            h = t.hediffSet.GetFirstHediffOfDef(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Fixed) as Hediff_WithGiver;
            if (h != null)
            {
                h.giver = null;
                t.RemoveHediff(h);
            }
        }*/

        public void AddEffect()
        {
            var t = Victim.health;


            //给予固定状态
            Hediff_Fixed hediffTF =
                (Hediff_Fixed)HediffMaker.MakeHediff(HediffDefOf.COF_Torture_Fixed, Victim);
            hediffTF.Giver = Parent;
            t.AddHediff(hediffTF);


            //给予附加hediff
            if (Props.hediff == null)
                return;
            HediffDef hediffDef = Props.hediff;
            Hediff hediff = t.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (hediff != null)
                return;
            Hediff_WithGiver hediffAdd;
            if (Props.part == null)
            {
                hediffAdd = (Hediff_WithGiver)HediffMaker.MakeHediff(hediffDef, Victim);
            }
            else
            {
                var bodyPart = t.hediffSet.GetNotMissingParts().FirstOrFallback(
                    p => p.def == Props.part);
                hediffAdd = (Hediff_WithGiver)HediffMaker.MakeHediff(hediffDef, Victim, bodyPart);
            }

            hediffAdd.Giver = Parent;
            t.AddHediff(hediffAdd);
        }
    }
}