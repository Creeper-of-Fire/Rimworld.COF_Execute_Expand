using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using COF_Torture.Body;
using COF_Torture.Data;
using COF_Torture.Utility;
using COF_Torture.Utility.DefOf;
using Verse;
using Verse.Noise;

namespace COF_Torture.Hediffs
{
    public class Hediff_COF_Torture_IsAbusing : Hediff
    {
        public Dictionary<Hediff, ExposableList<ActionWithBar>> ActionList =
            new Dictionary<Hediff, ExposableList<ActionWithBar>>();

        private ExposableList<ActionWithBar> nowActions;
        private Hediff nowHediff;

        private ActionWithBar nowAction;
        private bool started = false;

        public void Start()
        {
            started = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref started, "started", false);
            Scribe_Collections.Look(ref ActionList, "ActionList", LookMode.Reference, LookMode.Deep);
            Scribe_References.Look(ref nowHediff, "nowHediff");
            Scribe_Deep.Look(ref nowActions, "nowActions");
            Scribe_Deep.Look(ref nowAction, "nowAction");
        }

        public int Count
        {
            get
            {
                if (ActionList.NullOrEmpty())
                    return 0;
                int count = 0;
                foreach (var pair in ActionList)
                {
                    count += pair.Value.list.Count;
                }

                return count;
            }
        }

        public override string Description
        {
            get
            {
                var str = new StringBuilder();
                foreach (var actionWithBars in ActionList.Values)
                {
                    foreach (var actionWithBar in actionWithBars.list)
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
            if (!started)
                return;
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

            if (pawn.pather.Moving)
            {
                Break();
                return;
            }

            if (ShouldRemove)
            {
                return;
            }

            if (ActionList.NullOrEmpty())
            {
                Break();
                return;
            }

            if (nowHediff != null && !pawn.health.hediffSet.GetNotMissingParts().Contains(nowHediff.Part))
            {
                nowAction = null;
                ActionList.Remove(nowHediff);
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
                if (nowHediff == null) //|| nowActions.NullOrEmpty())
                {
                    nowHediff = ActionList.First().Key;
                    nowActions = ActionList.First().Value;
                    return;
                }

                //情况2，nowActions为空，移除的同时获取新nowActions
                if (nowActions.list.NullOrEmpty())
                {
                    ActionList.Remove(nowHediff);
                    nowHediff = null;
                    nowActions = null;
                    return;
                }

                //情况3，nowAction不存在，获取新nowAction
                if (nowAction == null)
                {
                    nowAction = nowActions.list[0];
                    return;
                }

                //情况4，nowAction需要结束
                if (nowAction != null && nowAction.progress >= 1f)
                {
                    nowAction.EndDirect();
                    nowAction = null;
                    nowActions.list.RemoveAt(0);
                }
            }
        }

        private void Break()
        {
            nowAction?.EndDirect();
            ActionList.Clear();
            Severity = 0f;
            started = false;
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