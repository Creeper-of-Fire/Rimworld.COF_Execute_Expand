using UnityEngine;
using Verse;

namespace COF_Torture.HediffComp
{
    public class HediffCompProperties_BecomePermanent : HediffCompProperties
    {
        public float WoundCrackingProbabilityPerHour = 0f;
        [MustTranslate] public string permanentLabel = "";
        [MustTranslate] public string permanentLabelExtraDescription="";

        public HediffCompProperties_BecomePermanent() => compClass = typeof(HediffComp_BecomePermanent);
    }
    
    /// <summary>
    /// 处刑器具刚刚安装上时会流血，之后转为永久伤口。
    /// </summary>
    public class HediffComp_BecomePermanent : HediffComp_GetsPermanent
    {
        public new HediffCompProperties_BecomePermanent Props => (HediffCompProperties_BecomePermanent)props;

        public override string CompDescriptionExtra
        {
            get
            {
                if (IsPermanent)
                    return Props.permanentLabelExtraDescription;
                return null;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Props.WoundCrackingProbabilityPerHour > (double)0f)
                if (Pawn.IsHashIntervalTick(2500))
                {
                    if (Props.WoundCrackingProbabilityPerHour >= (double)Random.value)
                    {
                        IsPermanent = true;
                        parent.Severity = parent.def.initialSeverity;
                    }
                }
        }

        public override void CompPostInjuryHeal(float amount)
        {
            if (IsPermanent)
                return;
            if (parent.Severity > 0.0f)
                return;
            parent.Severity = parent.def.initialSeverity / 2;
            IsPermanent = true;
            Pawn.health.Notify_HediffChanged(parent);
        }
    }
}