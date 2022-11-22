using System;
using System.Collections.Generic;
using COF_Torture.Dialog;
using COF_Torture.ModSetting;
using COF_Torture.Patch;
using RimWorld;
using Verse;

namespace COF_Torture.Hediffs
{
    public class Hediff_Fixed : Hediff_WithGiver
    {
        public int tickNext;
        private float Thirst;
        private float Rest;
        private float Food;
        private float Joy;
        private float Bladder;
        private float Hygiene;
        private const float lowFloatOfNeeds = 0.7f;

        public override void PostMake()
        {
            this.Severity = 1f;
            this.SetNextTick();
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            if (pawn.needs.food != null) Food = pawn.needs.food.CurLevel;
            if (pawn.needs.joy != null) Joy = pawn.needs.joy.CurLevel;
            if (pawn.needs.rest != null) Rest = pawn.needs.rest.CurLevel;
            Need need;
            if (SettingPatch.DubsBadHygieneThirstIsActive)
            {
                need = pawn.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == SettingPatch.DBHThirstNeed));
                if (need != null) Thirst = need.CurLevel;
            }

            if (SettingPatch.DubsBadHygieneIsActive)
            {
                need = pawn.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == SettingPatch.BladderNeed));
                if (need != null) Bladder = need.CurLevel;
                need = pawn.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == SettingPatch.HygieneNeed));
                if (need != null) Hygiene = need.CurLevel;
            }
            //Log.Message(""+Food+Joy+Rest);

            base.PostAdd(dinfo);
        }

        public override void PostRemoved()
        {
            if (!ModSettingMain.Instance.Setting.isFeed)
            {
                if (pawn.needs.food != null) pawn.needs.food.CurLevel = Food;
                if (pawn.needs.joy != null) pawn.needs.joy.CurLevel = Joy;
                if (pawn.needs.rest != null) pawn.needs.rest.CurLevel = Rest;
                Need need;
                if (SettingPatch.DubsBadHygieneThirstIsActive)
                {
                    need = pawn.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == SettingPatch.DBHThirstNeed));
                    if (need != null) need.CurLevel = Thirst;
                }

                if (SettingPatch.DubsBadHygieneIsActive)
                {
                    need = pawn.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == SettingPatch.BladderNeed));
                    if (need != null) need.CurLevel = Bladder;
                    need = pawn.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == SettingPatch.HygieneNeed));
                    if (need != null) need.CurLevel = Hygiene;
                }
            }

            base.PostRemoved();
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

            this.SatisfyFood();
            this.SatisfyJoy();
            this.SatisfyThirst();
            this.SatisfyBladder();
            this.SatisfyHygiene();

            this.SetNextTick();
        }

        public override bool ShouldRemove
        {
            get
            {
                if (Giver == null)
                    return true;
                return base.ShouldRemove;
            }
        }

        public void SatisfyFood()
        {
            Need_Food need = this.pawn.needs.TryGetNeed<Need_Food>();
            if (need == null || (double)need.CurLevel >= lowFloatOfNeeds)
                return;
            this.pawn.needs.food.CurLevel += need.MaxLevel / 5f;
        }

        public void SatisfyJoy()
        {
            Need_Food need = this.pawn.needs.TryGetNeed<Need_Food>();
            if (need == null || (double)need.CurLevel >= lowFloatOfNeeds)
                return;
            this.pawn.needs.food.CurLevel += need.MaxLevel / 5f;
        }

        public void SatisfyThirst()
        {
            if (!SettingPatch.DubsBadHygieneThirstIsActive)
                return;
            Need need = this.pawn.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == SettingPatch.DBHThirstNeed));
            if (need == null || (double)need.CurLevel >= lowFloatOfNeeds)
                return;
            float num = need.MaxLevel / 5f;
            this.pawn.needs.TryGetNeed(need.def).CurLevel += num;
        }

        public void SatisfyBladder()
        {
            if (!SettingPatch.DubsBadHygieneIsActive)
                return;
            Need need = this.pawn.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == SettingPatch.HygieneNeed));
            if (need == null || (double)need.CurLevel >= lowFloatOfNeeds)
                return;
            float num = need.MaxLevel / 5f;
            this.pawn.needs.TryGetNeed(need.def).CurLevel += num;
        }

        public void SatisfyHygiene()
        {
            if (!SettingPatch.DubsBadHygieneIsActive)
                return;
            Need need = this.pawn.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == SettingPatch.BladderNeed));
            if (need == null || (double)need.CurLevel >= lowFloatOfNeeds)
                return;
            float num = need.MaxLevel / 5f;
            this.pawn.needs.TryGetNeed(need.def).CurLevel += num;
        }

        public void SetNextTick() => this.tickNext = Find.TickManager.TicksGame + 1000;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (base.GetGizmos() != null)
                foreach (var gizmo in base.GetGizmos())
                {
                    yield return gizmo;
                }
            if (ModSettingMain.Instance.Setting.controlMenuOn) yield break;
            foreach (var command in this.Gizmo_ReleaseBondageBed())
            {
                yield return command;
            }
        }
    }
}