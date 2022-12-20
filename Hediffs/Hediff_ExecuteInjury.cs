using COF_Torture.Dialog;
using COF_Torture.ModSetting;
using COF_Torture.Utility;
using Verse;

namespace COF_Torture.Hediffs
{
    public class Hediff_ExecuteInjury : Hediff_Injury, IWithThingGiver
    {
        protected Thing giver;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref giver, "giver");
        }

        public override bool TendableNow(bool ignoreTimer = false)
        {
            if (giver == null)
                return base.TendableNow(ignoreTimer);
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
            get => giver;
            set
            {
                if (value is ITortureThing iValue)
                    iValue.hasGiven.Add(this);
                giver = value;
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