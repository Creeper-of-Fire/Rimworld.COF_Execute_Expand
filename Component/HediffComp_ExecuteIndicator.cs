using System.Collections.Generic;
using COF_Torture.Data;
using COF_Torture.Dialog;
using COF_Torture.Hediffs;
using COF_Torture.ModSetting;
using COF_Torture.Things;
using COF_Torture.Utility;
using UnityEngine;
using Verse;

namespace COF_Torture.Component
{
    public class HediffCompProperties_ExecuteIndicator : HediffCompProperties
    {
        //public int ticksToCount = 100;

        //public int ticksToExecute = 9000;
        //public float severityToDeath = 10.0f;
        public HediffCompProperties_ExecuteIndicator() => compClass = typeof(HediffComp_ExecuteIndicator);
    }

    /// <summary>
    /// 处理绝大多数的处刑相关内容——特别是关于殖民者何时死亡
    /// </summary>
    public class HediffComp_ExecuteIndicator : HediffComp
    {
        public HediffCompProperties_ExecuteIndicator Props => (HediffCompProperties_ExecuteIndicator)props;
        public Hediff_WithGiver Parent => (Hediff_WithGiver)parent;

        private int ticksLeftToCount;

        private float severityAdd;

        private float severityToDeath;

        /// <summary>
        /// 是否在处刑中
        /// </summary>
        public bool isInProgress;

        private const int ticksToCount = 120;
        //private Hediff bloodLoss;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref isInProgress, "isInProgress");
        }
        
        protected virtual void SeverityProcess()
        {
            if (severityToDeath <= 0f)
            {
                if (Parent.def.lethalSeverity <= 0f)
                {
                    ModLog.Error("错误：" + Parent + "是用于处理处刑效果的hediff，但是没有致死严重度数据");
                    severityToDeath = 10f;
                }
                else
                    severityToDeath = Parent.def.lethalSeverity;
            }

            if (severityAdd == 0f)
                severityAdd = severityToDeath /
                              ((float)ModSettingMain.Instance.Setting.executeHours * 2500 / ticksToCount);
            //上面为初始化严重度相关设置
            if (parent.Severity >= (double)severityToDeath)
            {
                var a = (Building_TortureBed)Parent.Giver;
                a.isUsed = true;
                if (Parent.Giver is Building_TortureBed bT && !bT.isSafe)
                    TortureUtility.KillVictimDirect(Pawn);
            }
            else
            {
                parent.Severity += severityAdd;
            }
        }

        /// <summary>
        /// 处理是否故障启动
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        protected void mistakeStartUp()
        {
            if (ModSettingMain.Instance.Setting.mistakeStartUp != 0.0f)
                if (Pawn.IsHashIntervalTick(2500))
                {
                    if (ModSettingMain.Instance.Setting.mistakeStartUp >= Random.value)
                        StartProgress();
                }
        }

        public sealed override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            mistakeStartUp();
            if (!isInProgress) return;
            ticksLeftToCount--;
            if (ticksLeftToCount > 0) //多次CompPostTick执行一次
                return;
            ticksLeftToCount = ticksToCount; //重置计时器
            SeverityProcess();
            if (!ModSettingMain.Instance.Setting.isImmortal) //如果会死，就改变死因为本comp造成
            {
                TortureUtility.ShouldNotDie(Pawn);
                if (TortureUtility.ShouldBeDead(Pawn))
                    TortureUtility.KillVictimDirect(Pawn);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            if (ModSettingMain.Instance.Setting.controlMenuOn) yield break;
            foreach (var command in this.Gizmo_StartAndStopExecute())
            {
                yield return command;
            }
        }

        /// <summary>
        /// 开始处刑
        /// </summary>
        public virtual void StartProgress()
        {
            Parent.GiverAsInterface.startExecuteProgress();
            isInProgress = true;
        }

        /// <summary>
        /// 暂停处刑
        /// </summary>
        public virtual void StopProgress()
        {
            Parent.GiverAsInterface.stopExecuteProgress();
            isInProgress = false;
        }
    }
}