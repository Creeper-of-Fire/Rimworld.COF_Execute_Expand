using System.Collections.Generic;
using COF_Torture.Dialog;
using COF_Torture.ModSetting;
using Verse;

namespace COF_Torture.HediffComp
{
    public class HediffCompProperties_SwitchAbleSeverity : HediffCompProperties
    {
        public HediffCompProperties_SwitchAbleSeverity() => compClass = typeof(HediffComp_SwitchAbleSeverity);
    }

    /// <summary>
    /// 可手动调节严重度
    /// </summary>
    public class HediffComp_SwitchAbleSeverity : Verse.HediffComp
    {
        public HediffCompProperties_SwitchAbleSeverity Props => (HediffCompProperties_SwitchAbleSeverity)props;
        public int stageLimit;
        public int stageMax => parent.def.stages.Count - 1;

        //public bool isUsing;
        //不知道是不是应该给这个comp加个开关

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref stageLimit, "stageLimit", defaultValue: 0);
            //Scribe_Values.Look<bool>(ref this.isUsing, "isUsing", defaultValue: false);
        }

        private float severityLimit
        {
            get
            {
                if (stageLimit < stageMax)
                    return parent.def.stages[stageLimit].minSeverity;
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
            if (stageLimit < stageMax)
                stageLimit += 1;
            parent.Severity = severityLimit;
            Notify_SexualHeatComp();
        }

        public void downStage()
        {
            if (stageLimit > 0)
                stageLimit -= 1;
            parent.Severity = severityLimit;
            Notify_SexualHeatComp();
        }

        public void Notify_SexualHeatComp()
        {
           var comp = parent.TryGetComp<HediffComp_GetSexualHeat>();
           if (comp !=null)
               comp.RefreshSexualHeatMaker();
        }
    }
}