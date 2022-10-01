using System;
using COF_Torture.Patch;
using RimWorld;
using Verse;


namespace COF_Torture.Component
{
    public class HediffComp_OrgasmIndicator : HediffComp //处理性兴奋，如果性兴奋满值就高潮
    {
        public HediffCompProperties_OrgasmIndicator Props => (HediffCompProperties_OrgasmIndicator)this.props;

        private int ticksToCount;

        private Verse.Hediff HediffOrgasm = null;

        public void Orgasm()
        {
            if (SettingPatch.RimJobWorldIsActive)
            {
                Need need = this.Pawn.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == SettingPatch.Sex));
                need.CurLevel += this.Props.satisfySexNeedWhenOrgasm; //因为高潮获得了性满足
            }
            if (this.Pawn.health.hediffSet.HasHediff(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Orgasm))
            {
                if (this.HediffOrgasm == null)
                {
                    this.HediffOrgasm = this.Pawn.health.hediffSet.GetFirstHediffOfDef(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Orgasm);
                }
                this.HediffOrgasm.Severity += 1;
                HediffComp_Disappears comps1 = this.HediffOrgasm.TryGetComp<HediffComp_Disappears>();
                comps1.ticksToDisappear = comps1.Props.disappearsAfterTicks.RandomInRange;
            }
            //补充高潮状态，重置消失时间
            else
            {
                this.HediffOrgasm = this.Pawn.health.AddHediff(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Orgasm);
            }
        }

        public void IsOrgasm(HediffCompProperties_OrgasmIndicator props1)
        {
            if (props1.orgasmSeverity < this.parent.Severity)
            {
                this.Orgasm();
                if (props1.canMultiOrgasm)
                {
                    this.parent.Severity -= props1.orgasmSeverity;
                    this.IsOrgasm(props1);
                    //如果剩下的高潮指示条还能允许一次高潮，就不重置它
                }
                this.parent.Severity = 0.01f;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            this.ticksToCount--;
            if (this.ticksToCount > 0) //多次CompPostTick执行一次
                return;
            //Log.Message("11111111");
            HediffCompProperties_OrgasmIndicator props1 = this.Props;
            this.ticksToCount = props1.ticksToCount;
            IsOrgasm(props1);
            /*else if (this.Pawn.IsHashIntervalTick(200))
            {
                float num = props1.sexAdjustment;
                sex_need.CurLevel += num;
            }*/
        }
    }

    public class HediffCompProperties_OrgasmIndicator : HediffCompProperties
    {
        //public float sexAdjustment;
        //public HediffDef HediffTriggered;
        public float satisfySexNeedWhenOrgasm;
        public float orgasmSeverity = 1;
        public bool canMultiOrgasm = false;
        public int ticksToCount = 1000;

        public HediffCompProperties_OrgasmIndicator() => this.compClass = typeof(HediffComp_OrgasmIndicator);
    }
}