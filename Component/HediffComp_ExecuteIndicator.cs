using System.Collections.Generic;
using COF_Torture.Hediffs;
using COF_Torture.ModSetting;
using COF_Torture.Patch;
using COF_Torture.Things;
using RimWorld;
using Verse;
using HediffDefOf = RimWorld.HediffDefOf;

namespace COF_Torture.Component
{
    public class HediffCompProperties_ExecuteIndicator : HediffCompProperties
    {
        public int ticksToCount = 100;
        public int ticksToExecute = 9000;
        public float severityToDeath = 10.0f;

        public LetterDef letter;
        public HediffCompProperties_ExecuteIndicator() => this.compClass = typeof(HediffComp_ExecuteIndicator);
    }

    public class HediffComp_ExecuteIndicator : HediffComp
    {
        public HediffCompProperties_ExecuteIndicator Props => (HediffCompProperties_ExecuteIndicator)this.props;
        public Hediff_Torture Parent => (Hediff_Torture)this.parent;

        private int ticksToCount;

        private float severityAdd;

        public override void CompPostTick(ref float severityAdjustment)
        {
            this.ticksToCount--;
            if (this.ticksToCount > 0) //多次CompPostTick执行一次
                return;
            HediffCompProperties_ExecuteIndicator props1 = this.Props;
            this.ticksToCount = props1.ticksToCount;
            severityAdd = props1.severityToDeath / ((float)props1.ticksToExecute / props1.ticksToCount);
            this.parent.Severity += severityAdd;
            if (this.parent.Severity + 0.01f > props1.severityToDeath)
            {
                var a = (Building_TortureBed)this.Parent.giver;
                a.isUsed = true;
                if (ModSettingMain.Instance.Setting.isSafe)
                {
                    if (this.Def.injuryProps.bleedRate != 0.0f)
                    {
                        var BL = this.Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
                        if (BL.Severity > 0.9f)
                            BL.Severity = 0.9f;
                    }
                    this.parent.Severity = (props1.severityToDeath - 0.01f);
                }
                else
                    this.KillByExecute();
            }
        }

        public void KillByExecute()
        {
            //a.ChangeGraphic();
            if (SettingPatch.RimJobWorldIsActive)
            {
                var execute = Damages.DamageDefOf.Execute;
                var dInfo = new DamageInfo(execute, 1);
                var dHediff = HediffMaker.MakeHediff(Hediffs.HediffDefOf.COF_Torture_Licentious, this.Pawn);
                this.parent.pawn.Kill(dInfo, dHediff);
            }
            else
            {
                this.parent.pawn.Kill(new DamageInfo(), this.Parent);
            }
        }
    }
}