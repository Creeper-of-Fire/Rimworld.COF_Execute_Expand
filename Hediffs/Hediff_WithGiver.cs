using Verse;

namespace COF_Torture.Hediffs
{
    public class Hediff_WithGiver : HediffWithComps, IWithGiver
    {
        private Thing giver;

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

        public Thing Giver
        {
            get => this.giver;
            set => this.giver = value;
        }

        public ITortureThing GiverAsInterface
        {
            get => (ITortureThing)giver;
            set => giver = (Thing)value;
        }
    }

    public abstract class WithGiver
    {
        public Thing giver;
    }
}