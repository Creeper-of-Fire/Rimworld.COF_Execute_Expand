using COF_Torture.ModSetting;
using COF_Torture.Things;
using Verse;

namespace COF_Torture.Hediffs
{
    public class Hediff_ExecuteInjury: Hediff_Injury
    {
        public Thing giver;//懒得多重继承，摆烂了

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
    }
}