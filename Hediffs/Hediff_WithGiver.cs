using Verse;

namespace COF_Torture.Hediffs
{
    public class Hediff_WithGiver : HediffWithComps
    {
        public Thing giver;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Thing>(ref this.giver, "giver");
        }

        public override bool TendableNow(bool ignoreTimer = false)
        {
            if (giver == null)
                return base.TendableNow(ignoreTimer);
            else
                return false;
        }
    }
}