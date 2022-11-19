using System.Collections.Generic;
using COF_Torture.Hediffs;
using COF_Torture.ModSetting;
using COF_Torture.Patch;
using COF_Torture.Things;
using RimWorld;
using UnityEngine;
using Verse;
using HediffDefOf = RimWorld.HediffDefOf;

namespace COF_Torture.Component
{
    public class HediffCompProperties_ExecuteIndicator : HediffCompProperties
    {
        //public int ticksToCount = 100;

        //public int ticksToExecute = 9000;
        //public float severityToDeath = 10.0f;
        public HediffCompProperties_ExecuteIndicator() => this.compClass = typeof(HediffComp_ExecuteIndicator);
    }

    public class HediffComp_ExecuteIndicator : HediffComp //最重要的comp，处理绝大多数的处刑相关内容——特别是关于殖民者何时死亡
    {
        public HediffCompProperties_ExecuteIndicator Props => (HediffCompProperties_ExecuteIndicator)this.props;
        public Hediff_WithGiver Parent => (Hediff_WithGiver)this.parent;

        private int ticksLeftToCount;

        private float severityAdd;

        private float severityToDeath;

        public bool isInProgress;

        private const int ticksToCount = 120;
        //private Hediff bloodLoss;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref isInProgress, "isInProgress", false);
        }

        public virtual void SeverityProcess()
        {
            if (severityToDeath <= 0f)
            {
                if (this.Parent.def.lethalSeverity <= 0f)
                {
                    Log.Error("[COF_TORTURE]错误：" + this.Parent + "是用于处理处刑效果的hediff，但是没有致死严重度数据");
                    severityToDeath = 10f;
                }
                else
                    severityToDeath = this.Parent.def.lethalSeverity;
            }

            if (severityAdd == 0f)
                severityAdd = severityToDeath /
                              ((float)ModSettingMain.Instance.Setting.executeHours * 2500 / ticksToCount);
            //上面为初始化严重度相关设置
            if ((double)this.parent.Severity >= (double)severityToDeath)
            {
                var a = (Building_TortureBed)this.Parent.Giver;
                a.isUsed = true;
                if (this.Parent.Giver is Building_TortureBed bT && !bT.isSafe)
                    this.KillByExecute();
            }
            else
            {
                this.parent.Severity += severityAdd;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (ModSettingMain.Instance.Setting.mistakeStartUp != 0.0f)
                if (this.Pawn.IsHashIntervalTick(2500))
                {
                    if (ModSettingMain.Instance.Setting.mistakeStartUp >= Random.value)
                        StartProgress();
                }

            if (!isInProgress) return;
            this.ticksLeftToCount--;
            if (this.ticksLeftToCount > 0) //多次CompPostTick执行一次
                return;
            this.ticksLeftToCount = ticksToCount; //重置计时器
            SeverityProcess();
            if (!ModSettingMain.Instance.Setting.isImmortal) //如果会死，就改变死因为本comp造成
            {
                ShouldNotDie(this.Pawn);
                if (ShouldBeDead(this.Pawn))
                    KillByExecute();
            }
        }

        public static void ShouldNotDie(Pawn p)
        {
            var bloodLoss = p.health.hediffSet.GetFirstHediffOfDef(RimWorld.HediffDefOf.BloodLoss);
            if (bloodLoss != null)
                if (bloodLoss.Severity > 0.9f)
                    bloodLoss.Severity = 0.9f;
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
        }

        public static bool ShouldBeDead(Pawn pawn)
        {
            var health = pawn.health;
            if (health.Dead)
                return true;
            for (int index = 0; index < health.hediffSet.hediffs.Count; ++index)
            {
                if (health.hediffSet.hediffs[index].CauseDeathNow())
                    return true;
            }

            if (health.ShouldBeDeadFromRequiredCapacity() != null)
                return true;
            if ((double)PawnCapacityUtility.CalculatePartEfficiency(health.hediffSet, pawn.RaceProps.body.corePart) <=
                0.0)
            {
                if (DebugViewSettings.logCauseOfDeath)
                    Log.Message("CauseOfDeath: zero efficiency of " + pawn.RaceProps.body.corePart.Label);
                return true;
            }

            return health.ShouldBeDeadFromLethalDamageThreshold();
        }


        public void KillByExecute()
        {
            if (this.Pawn.Dead)
                Log.Error("try to kill a dead pawn");
            //Log.Message(this.Parent.giver.GetVictim()+"KillVictim");
            if (this.Parent.Giver is Building_TortureBed bT)
                bT.KillVictim();
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            if (base.CompGetGizmos() != null)
                foreach (var gizmo in base.CompGetGizmos())
                {
                    yield return gizmo;
                }
        }

        public virtual void StartProgress()
        {
            this.Parent.GiverAsInterface.startExecuteProgress();
            isInProgress = true;
        }

        public virtual void StopProgress()
        {
            this.Parent.GiverAsInterface.stopExecuteProgress();
            isInProgress = false;
        }
    }
}