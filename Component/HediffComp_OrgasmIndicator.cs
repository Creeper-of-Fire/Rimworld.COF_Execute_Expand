using COF_Torture.Data;
using COF_Torture.Utility;
using Verse;


namespace COF_Torture.Component
{
    public class HediffComp_OrgasmIndicator : HediffComp //性兴奋指示条，如果性兴奋满值就高潮
    {
        public HediffCompProperties_OrgasmIndicator Props => (HediffCompProperties_OrgasmIndicator)this.props;
        public const int orgasmSeverity = 1;
        private int ticksToCount;
        private void IsOrgasm(HediffCompProperties_OrgasmIndicator props1)
        {
            if (orgasmSeverity < this.parent.Severity)
            {
                TortureUtility.Orgasm(this.Pawn);
                if (props1.canMultiOrgasm)
                {
                    this.parent.Severity -= orgasmSeverity;
                    this.IsOrgasm(props1);
                    //如果剩下的高潮指示条还能允许一次高潮，就不重置它
                }

                this.parent.Severity = 0.01f;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            this.ticksToCount--;
            if (this.ticksToCount > 0) //多次CompPostTick执行一次
                return;
            HediffCompProperties_OrgasmIndicator props1 = this.Props;
            this.ticksToCount = props1.ticksToCount;
            IsOrgasm(props1);
        }
    }

    public class HediffCompProperties_OrgasmIndicator : HediffCompProperties
    {
        //public float sexAdjustment;
        //public HediffDef HediffTriggered;
        //public float orgasmSeverity = 1;
        public bool canMultiOrgasm = false;
        public int ticksToCount = 1000;

        public HediffCompProperties_OrgasmIndicator() => this.compClass = typeof(HediffComp_OrgasmIndicator);
    }
}