using COF_Torture.Things;
using Verse;

namespace COF_Torture.Component
{
    public abstract class HediffComp_ExecuteIndicatorWithEffector : HediffComp_ExecuteIndicator
    {
        protected abstract HediffComp GetChildComp();

        public override void StartProgress()
        {
            var comp = (HediffComp_ExecuteEffector)GetChildComp();
            if (comp != null)
            {
                comp.startExecuteProcess();
                this.Parent.GiverAsInterface?.startExecuteProgress();
                isInProgress = true;
            }
            else
            {
                Log.Error("[COF_Torture]类型" + this.GetType() + "未找到对应的comp");
            }
        }

        public override void StopProgress()
        {
            var comp = (HediffComp_ExecuteEffector)GetChildComp();
            if (comp != null)
            {
                comp.stopExecuteProcess();
                this.Parent.GiverAsInterface?.stopExecuteProgress();
                isInProgress = false;
            }
            else
            {
                Log.Error("[COF_Torture]类型" + this.GetType() + "未找到对应的comp");
            }
        }
    }
    public class HediffCompProperties_ExecuteIndicatorAddHediff : HediffCompProperties
    {
        public HediffCompProperties_ExecuteIndicatorAddHediff() =>
            this.compClass = typeof(HediffComp_ExecuteIndicatorAddHediff);
    }

    public class HediffComp_ExecuteIndicatorAddHediff : HediffComp_ExecuteIndicatorWithEffector
    {
        protected sealed override HediffComp GetChildComp()
        {
            var comp = this.Parent.TryGetComp<HediffComp_ExecuteEffector_AddHediff>();
            return comp;
        }
    }
    
    public class HediffCompProperties_ExecuteIndicatorMincer : HediffCompProperties
    {
        public HediffCompProperties_ExecuteIndicatorMincer() => this.compClass = typeof(HediffComp_ExecuteIndicatorMincer);
    }

    public class HediffComp_ExecuteIndicatorMincer : HediffComp_ExecuteIndicatorWithEffector
    {
        public bool isButcherDone;
        private const int ticksToCount = 120;
        //private Hediff bloodLoss;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref isButcherDone, "isButcherDone", false);
        }

        protected override void SeverityProcess()
        {
            if (!this.isButcherDone) return;
            if (this.Parent.Giver is Building_TortureBed bT && !bT.isSafe)
                TortureUtility.KillVictimDirect(this.Pawn);
        }

        protected sealed override HediffComp GetChildComp()
        {
            var comp = this.Parent.TryGetComp<HediffComp_ExecuteEffector_Mincer>();
            return comp;
        }
    }
    
}