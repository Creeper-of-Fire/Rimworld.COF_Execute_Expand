using RimWorld;
using Verse;

namespace COF_Torture.Component
{
    public class CompProperties_WithHediff_Apparel : CompProperties
    {
        public HediffDef hediff;
        public BodyPartDef part;

        public CompProperties_WithHediff_Apparel() => this.compClass = typeof (CompWithHediff_Apparel);
    }
    
    public class CompWithHediff_Apparel : ThingComp
    {
        private CompProperties_WithHediff_Apparel Props => (CompProperties_WithHediff_Apparel) this.props;

        public override void Notify_Equipped(Pawn pawn)
        {
            if (pawn.health.hediffSet.GetFirstHediffOfDef(this.Props.hediff) != null)
                return;
            HediffComp_RemoveIfApparelDropped comp = pawn.health.AddHediff(this.Props.hediff, pawn.health.hediffSet.GetNotMissingParts().FirstOrFallback<BodyPartRecord>((Func<BodyPartRecord, bool>) (p => p.def == this.Props.part))).TryGetComp<HediffComp_RemoveIfApparelDropped>();
            if (comp == null)
                return;
            comp.wornApparel = (Apparel) this.parent;
            base.Notify_Equipped(pawn);
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
        }
    }
}