using COF_Torture.Component;
using COF_Torture.Data;
using Verse;

namespace COF_Torture.Hediffs
{
    public class Hediff_Torture : Hediff_WithGiver
    {
        public override float BleedRate
        {
            get
            {
                if (Part == null)
                {
                    return 0.0f;
                }

                if (def.injuryProps == null)
                    return 0.0f;
                if (pawn.Dead ||
                    Part.def.IsSolid(Part,
                        pawn.health.hediffSet
                            .hediffs)) //|| this.pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(this.Part))
                    return 0.0f;
                try
                {
                    float bR = def.injuryProps.bleedRate;
                    return bR;
                }
                catch
                {
                    ModLog.Message_Start("错误：" + this + "没有设置流血的数值");
                    return 0.0f;
                }
            }
        }

        public override bool ShouldRemove
        {
            get
            {
                if (Giver == null)
                    return true;
                return base.ShouldRemove;
            }
        }

        //使用自带的方法来让小人死亡，所以重写CauseDeathNow（这是为了继续使用致死严重度来适配UI模组）
        public override bool CauseDeathNow()
        {
            return false;
        }

        /*public override string LabelBase
        {
            get
            {
                var labelInBrackets = base.LabelInBrackets;
                if (!labelInBrackets.NullOrEmpty())
                    return labelInBrackets;
                else
                    return base.LabelBase;
            }
        }*/

        public override string LabelInBrackets
        {
            get
            {
                string labelInBrackets = base.LabelInBrackets;
                string stringPercent = (Severity/def.lethalSeverity).ToStringPercent("F0");
                if (this.TryGetComp<HediffComp_ExecuteIndicator>() == null)
                    return labelInBrackets;
                if (labelInBrackets.NullOrEmpty())
                    return stringPercent;
                return labelInBrackets + " " + stringPercent;
            }
        }
    }
}