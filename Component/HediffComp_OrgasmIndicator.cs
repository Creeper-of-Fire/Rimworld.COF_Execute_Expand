using COF_Torture.Utility;
using Verse;

namespace COF_Torture.Component
{
    public class HediffComp_OrgasmIndicator : HediffComp //性兴奋指示条，如果性兴奋满值就高潮
    {
        public HediffCompProperties_OrgasmIndicator Props => (HediffCompProperties_OrgasmIndicator)props;
        public const int orgasmSeverity = 1;
        private int ticksToCount;
        private void IsOrgasm(HediffCompProperties_OrgasmIndicator props1)
        {
            if (orgasmSeverity < parent.Severity)
            {
                TortureUtility.Orgasm(Pawn);
                if (props1.canMultiOrgasm)
                {
                    parent.Severity -= orgasmSeverity;
                    IsOrgasm(props1);
                    //如果剩下的高潮指示条还能允许一次高潮，就不重置它
                }

                parent.Severity = 0.01f;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            ticksToCount--;
            if (ticksToCount > 0) //多次CompPostTick执行一次
                return;
            HediffCompProperties_OrgasmIndicator props1 = Props;
            ticksToCount = props1.ticksToCount;
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

        public HediffCompProperties_OrgasmIndicator() => compClass = typeof(HediffComp_OrgasmIndicator);
    }
}