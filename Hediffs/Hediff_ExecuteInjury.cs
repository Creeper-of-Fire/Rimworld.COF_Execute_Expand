using COF_Torture.ModSetting;
using Verse;

namespace COF_Torture.Hediffs
{
    public class Hediff_ExecuteInjury : Hediff_Injury, IWithGiver
    {
        protected Thing giver;

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

        public override bool ShouldRemove
        {
            get
            {
                if (ModSettingMain.Instance.Setting.isRemoveTempInjuries && giver == null)
                    return true;
                return base.ShouldRemove;
            }
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
}