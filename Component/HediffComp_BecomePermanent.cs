using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace COF_Torture.Component
{
    public class HediffCompProperties_BecomePermanent : HediffCompProperties
    {
        public float WoundCrackingProbabilityPerHour = 0f;
        [MustTranslate] public string permanentLabel;
        [MustTranslate] public string permanentLabelExtraDescription;

        public HediffCompProperties_BecomePermanent() => this.compClass = typeof(HediffComp_BecomePermanent);
    }
    public class HediffComp_BecomePermanent : HediffComp_GetsPermanent
    {
        public new HediffCompProperties_BecomePermanent Props => (HediffCompProperties_BecomePermanent)this.props;

        public override string CompDescriptionExtra
        {
            get
            {
                if (this.IsPermanent)
                    return this.Props.permanentLabelExtraDescription;
                return (string)null;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (this.Props.WoundCrackingProbabilityPerHour > (double)0f)
                if (this.Pawn.IsHashIntervalTick(2500))
                {
                    if (this.Props.WoundCrackingProbabilityPerHour >= (double)Random.value)
                    {
                        this.IsPermanent = true;
                        this.parent.Severity = this.parent.def.initialSeverity;
                    }
                }
        }

        public override void CompPostInjuryHeal(float amount)
        {
            if (this.IsPermanent)
                return;
            if (!(this.parent.Severity <= 0.0f))
                return;
            this.parent.Severity = this.parent.def.initialSeverity / 2;
            this.IsPermanent = true;
            this.Pawn.health.Notify_HediffChanged((Hediff)this.parent);
        }
    }
}