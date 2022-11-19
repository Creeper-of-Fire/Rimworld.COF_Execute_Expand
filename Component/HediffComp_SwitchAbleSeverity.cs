using System.Collections.Generic;
using System.Linq;
using COF_Torture.Hediffs;
using COF_Torture.ModSetting;
using COF_Torture.Patch;
using COF_Torture.Things;
using RimWorld;
using UnityEngine;
using Verse;

namespace COF_Torture.Component
{
    public class HediffCompProperties_SwitchAbleSeverity : HediffCompProperties
    {
        public HediffCompProperties_SwitchAbleSeverity() => this.compClass = typeof(HediffComp_SwitchAbleSeverity);
    }

    public class HediffComp_SwitchAbleSeverity : HediffComp
    {
        public HediffCompProperties_SwitchAbleSeverity Props => (HediffCompProperties_SwitchAbleSeverity)this.props;
        public int stageLimit;
        public int stageMax => parent.def.stages.Count - 1;

        public bool isUsing;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<int>(ref this.stageLimit, "stageLimit", defaultValue: 0);
            Scribe_Values.Look<bool>(ref this.isUsing, "isUsing", defaultValue: false);
        }

        public float severityLimit
        {
            get
            {
                if (this.stageLimit < this.stageMax)
                    return parent.def.stages[stageLimit].minSeverity;
                else
                    return parent.def.maxSeverity;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            return base.CompGetGizmos();
        }

        public void upStage()
        {
            if (this.stageLimit < this.stageMax)
                this.stageLimit += 1;
            parent.Severity = this.severityLimit;
        }

        public void downStage()
        {
            if (this.stageLimit > 0)
                this.stageLimit -= 1;
            parent.Severity = this.severityLimit;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            //Log.Message(stageMax + "");
            //Log.Message(severityLimit + "");
            base.CompPostPostAdd(dinfo);
        }
    }
}