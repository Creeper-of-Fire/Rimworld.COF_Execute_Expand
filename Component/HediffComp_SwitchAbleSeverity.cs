using System.Collections.Generic;
using COF_Torture.Dialog;
using COF_Torture.Dialog.Units;
using COF_Torture.ModSetting;
using Verse;

namespace COF_Torture.Component
{
    public class HediffCompProperties_SwitchAbleSeverity : HediffCompProperties
    {
        public HediffCompProperties_SwitchAbleSeverity() => this.compClass = typeof(HediffComp_SwitchAbleSeverity);
    }

    /// <summary>
    /// 可手动调节严重度
    /// </summary>
    public class HediffComp_SwitchAbleSeverity : HediffComp
    {
        public HediffCompProperties_SwitchAbleSeverity Props => (HediffCompProperties_SwitchAbleSeverity)this.props;
        public int stageLimit;
        public int stageMax => parent.def.stages.Count - 1;

        //public bool isUsing;
        //不知道是不是应该给这个comp加个开关

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref this.stageLimit, "stageLimit", defaultValue: 0);
            //Scribe_Values.Look<bool>(ref this.isUsing, "isUsing", defaultValue: false);
        }

        private float severityLimit
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
            if (ModSettingMain.Instance.Setting.controlMenuOn) yield break;
            foreach (var command in this.Gizmo_StageUpAndDown())
            {
                yield return command;
            }
        }
        public void upStage()
        {
            if (this.stageLimit < this.stageMax)
                this.stageLimit += 1;
            parent.Severity = this.severityLimit;
            Notify_SexualHeatComp();
        }

        public void downStage()
        {
            if (this.stageLimit > 0)
                this.stageLimit -= 1;
            parent.Severity = this.severityLimit;
            Notify_SexualHeatComp();
        }

        public void Notify_SexualHeatComp()
        {
           var comp = this.parent.TryGetComp<HediffComp_GetSexualHeat>();
           if (comp !=null)
               comp.RefreshSexualHeatMaker();
        }
    }
}