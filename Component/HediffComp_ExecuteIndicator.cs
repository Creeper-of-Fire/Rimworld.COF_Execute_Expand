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
        //public int ticksToExecute = 9000;
        //public float severityToDeath = 10.0f;

        public LetterDef letter;
        public HediffCompProperties_ExecuteIndicator() => this.compClass = typeof(HediffComp_ExecuteIndicator);
    }

    public class HediffComp_ExecuteIndicator : HediffComp
    {
        public HediffCompProperties_ExecuteIndicator Props => (HediffCompProperties_ExecuteIndicator)this.props;
        public Hediff_WithGiver Parent => (Hediff_WithGiver)this.parent;

        private int ticksToCount;

        private float severityAdd;

        private float severityToDeath;

        public bool isButcherDone;
        //private Hediff bloodLoss;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref isButcherDone, "isButcherDone", false);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            this.ticksToCount--;
            if (this.ticksToCount > 0) //多次CompPostTick执行一次
                return;
            HediffCompProperties_ExecuteIndicator props1 = this.Props;
            this.ticksToCount = props1.ticksToCount;

            if (isButcherDone == false && Parent.def != COF_Torture.Hediffs.HediffDefOf.COF_Torture_Mincer_Execute)
            {
                isButcherDone = true;
            }
            if (severityToDeath <= 0f)
            {
                if (this.Parent.def.lethalSeverity <= 0f)
                {
                    Log.Error("[COF_TORTURE]错误："+this.Parent+"是用于处理处刑效果的hediff，但是没有致死严重度数据");
                    severityToDeath = 10f;
                }
                else
                    severityToDeath = this.Parent.def.lethalSeverity;
            }

            if (severityAdd == 0f)
                severityAdd = severityToDeath /
                              ((float)ModSettingMain.Instance.Setting.executeHours * 2500 / props1.ticksToCount);
            //this.ShouldNotDie();//还是决定使用harmony赋予不死效果
            if ((double)this.parent.Severity >= (double) severityToDeath && isButcherDone)
            {
                var a = (Building_TortureBed)this.Parent.giver;
                a.isUsed = true;
                if (this.Parent.giver is Building_TortureBed bT && !bT.isSafe)
                    this.KillByExecute();
            }
            else
            {
                this.parent.Severity += severityAdd;
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
        }

        public override void CompPostPostRemoved()
        {
            if (this.Parent.giver is Building_TortureBed bT && bT.isSafe)
                ShouldNotDie();
            if (ShouldBeDead())//放下来时如果会立刻死，就改变死因为本comp造成
            {
                KillByExecute();
            }
            base.CompPostPostRemoved();
        }
        
        private bool ShouldBeDead()
        {
            var health = Pawn.health;
            if (health.Dead)
                return true;
            for (int index = 0; index < health.hediffSet.hediffs.Count; ++index)
            {
                if (health.hediffSet.hediffs[index].CauseDeathNow())
                    return true;
            }
            if (health.ShouldBeDeadFromRequiredCapacity() != null)
                return true;
            if ((double) PawnCapacityUtility.CalculatePartEfficiency(health.hediffSet, Pawn.RaceProps.body.corePart) <= 0.0)
            {
                if (DebugViewSettings.logCauseOfDeath)
                    Log.Message("CauseOfDeath: zero efficiency of " + Pawn.RaceProps.body.corePart.Label);
                return true;
            }
            return health.ShouldBeDeadFromLethalDamageThreshold();
        }

        public void ShouldNotDie()
        {
            if (this.Def.injuryProps.bleedRate != 0.0f)
            {
                var bloodLoss = this.Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
                if (bloodLoss.Severity > 0.9f)
                    bloodLoss.Severity = 0.9f;
            }
        }

        public void KillByExecute()
        {
            //a.ChangeGraphic();
            this.Parent.giver.victim = null;
            this.Parent.giver.isUsing = false;
            if (SettingPatch.RimJobWorldIsActive && Pawn.story.traits.HasTrait(TraitDefOf.Masochist))
            {
                var execute = Damages.DamageDefOf.Execute_Licentious;
                var dInfo = new DamageInfo(execute, 1);
                var dHediff = HediffMaker.MakeHediff(Hediffs.HediffDefOf.COF_Torture_Licentious, this.Pawn);
                this.parent.pawn.Kill(dInfo, dHediff);
            }
            else
            {
                var execute = Damages.DamageDefOf.Execute;
                var dInfo = new DamageInfo(execute, 1);
                var dHediff = HediffMaker.MakeHediff(this.Parent.def, this.Pawn);
                this.parent.pawn.Kill(dInfo, dHediff);
            }
        }
    }
}