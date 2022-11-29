using COF_Torture.Utility;
using Verse;

namespace COF_Torture.Hediffs
{
    public class Hediff_WithGiver : HediffWithComps, IWithGiver
    {
        private Thing giver;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.giver, "giver");
        }

        public override bool TendableNow(bool ignoreTimer = false)
        {
            if (giver == null)
                return base.TendableNow(ignoreTimer);
            else
                return false;
        }

        public Thing Giver
        {
            get => this.giver;
            set
            {
                if (value is ITortureThing iValue)
                    iValue.hasGiven.Add(this);
                this.giver = value;
            }
        }

        public ITortureThing GiverAsInterface
        {
            get => (ITortureThing)giver;
            set
            {
                value.hasGiven.Add(this);
                giver = (Thing)value;
            }
        }
    }
}