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
    public class HediffCompProperties_ApparelTortureHediff : HediffCompProperties
    {
        public HediffCompProperties_ApparelTortureHediff() => this.compClass = typeof(HediffComp_ApparelTortureHediff);
    }

    public class HediffComp_ApparelTortureHediff : HediffComp
    {
        public HediffCompProperties_ApparelTortureHediff Props => (HediffCompProperties_ApparelTortureHediff)this.props;
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
            {
                var stageUp = new Command_Action();
                stageUp.defaultLabel = "CT_stageUp".Translate();
                stageUp.defaultDesc = "CT_stageUp_desc".Translate();
                stageUp.icon = ContentFinder<Texture2D>.Get("COF_Torture/UI/SwitchStage");
                stageUp.action = this.upStage;
                if (this.stageLimit >= this.stageMax)
                    stageUp.Disable();
                yield return stageUp;
            }
            {
                var stageDown = new Command_Action();
                stageDown.defaultLabel = "CT_stageDown".Translate();
                stageDown.defaultDesc = "CT_stageDown_desc".Translate();
                stageDown.icon = ContentFinder<Texture2D>.Get("COF_Torture/UI/SwitchStage");
                stageDown.action = this.downStage;
                if (this.stageLimit <= 0)
                    stageDown.Disable();
                yield return stageDown;
            }
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