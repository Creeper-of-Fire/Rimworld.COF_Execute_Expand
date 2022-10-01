using Verse;

namespace COF_Torture.Hediffs
{
    public class Hediff_Torture: HediffWithComps
    {
        public Thing giver;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Thing>(ref this.giver, "giver");
        }

        public override float BleedRate
        {
            get
            {
                if (this.Part == null)
                {
                    return 0.0f;
                }
                if (this.def.injuryProps == null)
                    return 0.0f;
                if (this.pawn.Dead || this.Part.def.IsSolid(this.Part, this.pawn.health.hediffSet.hediffs))//|| this.pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(this.Part))
                    return 0.0f;
                try
                {
                    float bR = this.def.injuryProps.bleedRate;
                    return bR;
                }
                catch
                {
                    Log.Message("[COF_TORTURE]错误："+this+"没有设置流血的数值");
                    return 0.0f;
                }
            }
        }

        public override void PostTick()
        {
            base.PostTick();
            if (giver == null)
            {
                this.Severity = 0.0f;
            }
        }
        
        //使用自带的方法来让小人死亡，所以重写CauseDeathNow（这是为了继续使用致死严重度来适配UI模组）
        public override bool CauseDeathNow()
        {
            return false;
        }
    }
}