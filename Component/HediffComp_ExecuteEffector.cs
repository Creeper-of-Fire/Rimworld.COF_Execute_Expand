using Verse;

namespace COF_Torture.Component
{
    public abstract class HediffComp_ExecuteEffector : HediffComp
    {
        public bool isInProgress;

        public virtual void startExecuteProcess()
        {
            isInProgress = true;
        }

        public virtual void stopExecuteProcess()
        {
            isInProgress = false;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (!isInProgress) return;
            ProcessTick();
        }
        
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref isInProgress, "isInProgress");
        }

        protected abstract void ProcessTick();
    }
}