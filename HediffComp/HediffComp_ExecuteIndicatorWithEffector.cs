using COF_Torture.Data;
using COF_Torture.Things;
using COF_Torture.Utility;
using Verse;

namespace COF_Torture.HediffComp
{
    public abstract class HediffComp_ExecuteIndicatorWithEffector : HediffComp_ExecuteIndicator
    {
        protected abstract Verse.HediffComp GetChildComp();

        public override void StartProgress()
        {
            var comp = (HediffComp_ExecuteEffector)GetChildComp();
            if (comp != null)
            {
                comp.startExecuteProcess();
                Parent.GiverAsInterface?.startExecuteProgress();
                isInProgress = true;
            }
            else
            {
                ModLog.Error("类型" + GetType() + "未找到对应的comp");
            }
        }

        public override void StopProgress()
        {
            var comp = (HediffComp_ExecuteEffector)GetChildComp();
            if (comp != null)
            {
                comp.stopExecuteProcess();
                Parent.GiverAsInterface?.stopExecuteProgress();
                isInProgress = false;
            }
            else
            {
                ModLog.Error("类型" + GetType() + "未找到对应的comp");
            }
        }
    }
    public class HediffCompProperties_ExecuteIndicatorAddHediff : HediffCompProperties
    {
        public HediffCompProperties_ExecuteIndicatorAddHediff() =>
            compClass = typeof(HediffComp_ExecuteIndicatorAddHediff);
    }

    public class HediffComp_ExecuteIndicatorAddHediff : HediffComp_ExecuteIndicatorWithEffector
    {
        protected sealed override Verse.HediffComp GetChildComp()
        {
            var comp = Parent.TryGetComp<HediffComp_ExecuteEffector_AddHediff>();
            return comp;
        }
    }
    
    public class HediffCompProperties_ExecuteIndicatorMincer : HediffCompProperties
    {
        public HediffCompProperties_ExecuteIndicatorMincer() => compClass = typeof(HediffComp_ExecuteIndicatorMincer);
    }

    public class HediffComp_ExecuteIndicatorMincer : HediffComp_ExecuteIndicatorWithEffector
    {
        public bool isButcherDone;
        private const int ticksToCount = 120;
        //private Hediff bloodLoss;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref isButcherDone, "isButcherDone");
        }

        protected override void SeverityProcess()
        {
            if (!isButcherDone) return;
            if (Parent.Giver is Building_TortureBed bT && !bT.isSafe)
                TortureUtility.KillVictimDirect(Pawn);
        }

        protected sealed override Verse.HediffComp GetChildComp()
        {
            var comp = Parent.TryGetComp<HediffComp_ExecuteEffector_Mincer>();
            return comp;
        }
    }
    
}