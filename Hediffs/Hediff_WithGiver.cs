using COF_Torture.Things;
using Verse;

namespace COF_Torture.Hediffs
{
    public class Hediff_WithGiver : HediffWithComps
    {
        public Building_TortureBed giver;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Building_TortureBed>(ref this.giver, "giver");
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