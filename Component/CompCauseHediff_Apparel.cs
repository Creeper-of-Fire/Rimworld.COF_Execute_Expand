using System;
using RimWorld;
using Verse;

namespace COF_Torture.Component
{
    public class CompCauseHediff_Apparel : ThingComp
    {
        private CompProperties_CauseHediff_Apparel Props => (CompProperties_CauseHediff_Apparel)this.props;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (pawn.health.hediffSet.GetFirstHediffOfDef(this.Props.hediff) != null)
                return;
            pawn.health.AddHediff(this.Props.hediff,
                pawn.health.hediffSet.GetNotMissingParts()
                    .FirstOrFallback<BodyPartRecord>((Func<BodyPartRecord, bool>)(p => p.def == this.Props.part)));
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            Hediff h = pawn.health.hediffSet.GetFirstHediffOfDef(this.Props.hediff);
            if (h != null)
                pawn.health.RemoveHediff(h);
        }
    }

    public class CompProperties_CauseHediff_Apparel : CompProperties
    {
        public HediffDef hediff;
        public BodyPartDef part;

        public CompProperties_CauseHediff_Apparel() => this.compClass = typeof(CompCauseHediff_Apparel);
    }
}