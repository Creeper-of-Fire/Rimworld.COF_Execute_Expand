using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using COF_Torture.Data;
using COF_Torture.Dialog;
using COF_Torture.Utility;
using Verse;

namespace COF_Torture.Hediffs
{
    public class Hediff_COF_Torture_IsAbusing : Hediff
    {
        private readonly Dictionary<Hediff, List<ActionWithBar>> ActionList =
            new Dictionary<Hediff, List<ActionWithBar>>();

        private KeyValuePair<Hediff, List<ActionWithBar>> nowActions;
        private ActionWithBar nowAction;
        private const float SeverityPerHour = 5f;
        private const int tickPerHour = 2500;

        public override string Description
        {
            get
            {
                var str = new StringBuilder();
                foreach (var actionWithBars in ActionList.Values)
                {
                    foreach (var actionWithBar in actionWithBars)
                    {
                        str.Append(actionWithBar.label + "\n");
                    }
                }

                return str.ToString();
            }
        }

        public override void Tick()
        {
            base.Tick();
            DoTick(100);
        }

        public void DoTick(int depth)
        {
            depth--;
            if (depth <= 0)
            {
                ModLog.Warning("递归深度太深，自动跳出，检查你的输入是否正确");
                return;
            }

            if (this.pawn.pather.Moving)
            {
                Break();
                return;
            }

            if (this.ShouldRemove)
            {
                return;
            }

            if (ActionList.NullOrEmpty())
            {
                Break();
                return;
            }

            if (nowActions.Key != null && !pawn.health.hediffSet.GetNotMissingParts().Contains(nowActions.Key.Part))
            {
                nowAction = null;
                ActionList.Remove(nowActions.Key);
                nowActions = default;
            }

            TryNewAction();
            if (nowAction != null && nowAction.progress < 1f)
            {
                nowAction.Tick();
                return;
            }

            TryNewAction();
            DoTick(depth);

            void TryNewAction()
            {
                //情况0，直接跳出
                if (ActionList.NullOrEmpty())
                {
                    Break();
                    return;
                }

                //情况1，nowActions不存在，获取新nowActions
                if (nowActions.Key == null)
                {
                    nowActions = ActionList.First();
                    return;
                }

                //情况2，nowActions为空，移除的同时获取新nowActions
                if (nowActions.Value.NullOrEmpty())
                {
                    ActionList.Remove(nowActions.Key);
                    nowActions = default;
                    return;
                }

                //情况3，nowAction不存在，获取新nowAction
                if (nowAction == null)
                {
                    nowAction = nowActions.Value[0];
                    return;
                }

                //情况4，nowAction需要结束
                if (nowAction != null && nowAction.progress >= 1f)
                {
                    nowAction.EndDirect();
                    nowAction = null;
                    nowActions.Value.RemoveAt(0);
                    return;
                }
            }
        }

        public void AddAction(Hediff hediff, BodyPartRecord bodyPart)
        {
            var action = new Action(delegate
            {
                if (pawn.health.hediffSet.GetNotMissingParts().Contains(bodyPart))
                    pawn.health.AddHediff(hediff, bodyPart, dinfo: new DamageInfo());
            });
            var tick = hediff.Severity / SeverityPerHour * tickPerHour;
            ActionList.DictListAdd(hediff,
                new ActionWithBar(action, this.pawn, tick, bodyPart.Label + "," + hediff.Label));
        }

        private void Break()
        {
            nowAction?.EndDirect();
            ActionList.Clear();
            this.Severity = 0f;
        }

        public static Hediff_COF_Torture_IsAbusing AddHediff_COF_Torture_IsAbusing(Pawn pawn)
        {
            var h = pawn.health.hediffSet.GetFirstHediff<Hediff_COF_Torture_IsAbusing>();
            if (h == null)
            {
                h = (Hediff_COF_Torture_IsAbusing)HediffMaker.MakeHediff(HediffDefOf.COF_Torture_IsAbusing, pawn);
                if (h == null)
                    ModLog.Error("COF_Torture_IsAbusing不是Hediff_COF_Torture_IsAbusing类型");
                pawn.health.AddHediff(h);
            }

            return h;
        }
    }
}