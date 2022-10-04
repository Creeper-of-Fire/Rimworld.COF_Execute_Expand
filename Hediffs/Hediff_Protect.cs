using System;
using COF_Torture.ModSetting;
using COF_Torture.Patch;
using RimWorld;
using Verse;

namespace COF_Torture.Hediffs
{
    public class Hediff_Protect : Hediff_WithGiver
    {
        public int tickNext;

        public override void PostMake()
        {
            this.Severity = 1f;
            this.SetNextTick();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.tickNext, "tickNext", 1000, true);
        }

        public override void Tick()
        {
            if (Find.TickManager.TicksGame < this.tickNext)
                return;
            //this.HealWounds();
            if (ModSettingMain.Instance.Setting.isFeed)
            {
                this.SatisfyHunger();
                this.SatisfyThirst();
            }
            this.SetNextTick();
        }
        
        public override bool ShouldRemove
        {
            get
            {
                if (giver == null)
                    return true;
                return base.ShouldRemove;
            }
        }

        public void SatisfyHunger()
        {
            Need_Food need = this.pawn.needs.TryGetNeed<Need_Food>();
            if (need == null || (double)need.CurLevel >= 0.15)
                return;
            this.pawn.needs.food.CurLevel += need.MaxLevel / 5f;
        }

        public void SatisfyThirst()
        {
            if (!SettingPatch.DubsBadHygieneIsActive)
                return;
            Need need = this.pawn.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == SettingPatch.DBHThirst));
            if (need == null || (double)need.CurLevel >= 0.15)
                return;
            float num = need.MaxLevel / 5f;
            this.pawn.needs.TryGetNeed(need.def).CurLevel += num;
        }

        public void SetNextTick() => this.tickNext = Find.TickManager.TicksGame + 1000;
    }
}