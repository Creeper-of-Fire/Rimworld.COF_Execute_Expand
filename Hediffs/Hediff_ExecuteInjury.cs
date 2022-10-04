using COF_Torture.Things;
using Verse;

namespace COF_Torture.Hediffs
{
    public class Hediff_ExecuteInjury: Hediff_Injury
    {
        public Building_TortureBed giver;//懒得多重继承，摆烂了

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