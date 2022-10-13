using Verse;

namespace COF_Torture.Component
{
    public class HediffComp_SexualHeatWithPain : HediffComp //通过疼痛获得性兴奋
    {
        public HediffCompProperties_SexualHeatWithPain Props => (HediffCompProperties_SexualHeatWithPain)this.props;
        
        private int count1;
        private int count2;
        private float SexualHeatGet;
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            //Log.Message("11111111");
            this.count1++;
            this.count2++;
            if (this.count1 > 1000) //1000次CompPostTick执行一次
            {
                HediffCompProperties_SexualHeatWithPain props1 = this.Props;
                this.count1 = 0;
                float pain = this.Pawn.health.hediffSet.PainTotal;
                this.SexualHeatGet = pain * props1.SexualHeatConversionRate;
            }
            if (this.count2 > 100) //100次CompPostTick执行一次
            {
                this.count2 = 0;
                this.parent.Severity += this.SexualHeatGet;
            }
        }
    }
        
    public class HediffCompProperties_SexualHeatWithPain : HediffCompProperties
    {
        public float SexualHeatConversionRate = 0f;
        public HediffCompProperties_SexualHeatWithPain() => this.compClass = typeof(HediffComp_SexualHeatWithPain);
    }
}