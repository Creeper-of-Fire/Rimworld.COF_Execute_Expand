using System.Collections.Generic;
using COF_Torture.Data;
using COF_Torture.Dialog;
using COF_Torture.ModSetting;
using COF_Torture.Patch;
using COF_Torture.Utility;
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
            Severity = 1f;
            SetNextTick();
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            if (pawn.needs.food != null) Food = pawn.needs.food.CurLevel;
            if (pawn.needs.joy != null) Joy = pawn.needs.joy.CurLevel;
            if (pawn.needs.rest != null) Rest = pawn.needs.rest.CurLevel;
            Need need;
            if (SettingPatch.DubsBadHygieneThirstIsActive)
            {
                need = pawn.needs.AllNeeds.Find(x => x.def == SettingPatch.DBHThirstNeed);
                if (need != null) Thirst = need.CurLevel;
            }

            if (SettingPatch.DubsBadHygieneIsActive)
            {
                need = pawn.needs.AllNeeds.Find(x => x.def == SettingPatch.BladderNeed);
                if (need != null) Bladder = need.CurLevel;
                need = pawn.needs.AllNeeds.Find(x => x.def == SettingPatch.HygieneNeed);
                if (need != null) Hygiene = need.CurLevel;
            }

            pawn.GetPawnData().IsFixed = true;

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
                    need = pawn.needs.AllNeeds.Find(x => x.def == SettingPatch.DBHThirstNeed);
                    if (need != null) need.CurLevel = Thirst;
                }

                if (SettingPatch.DubsBadHygieneIsActive)
                {
                    need = pawn.needs.AllNeeds.Find(x => x.def == SettingPatch.BladderNeed);
                    if (need != null) need.CurLevel = Bladder;
                    need = pawn.needs.AllNeeds.Find(x => x.def == SettingPatch.HygieneNeed);
                    if (need != null) need.CurLevel = Hygiene;
                }
            }

            pawn.GetPawnData().IsFixed = false;

            base.PostRemoved();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref tickNext, "tickNext", 1000, true);
        }

        public override void Tick()
        {
            //base.Tick();
            if (Find.TickManager.TicksGame < tickNext)
                return;
            //this.HealWounds();

            SatisfyFood();
            SatisfyJoy();
            SatisfyThirst();
            SatisfyBladder();
            SatisfyHygiene();

            SetNextTick();
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
            Need_Food need = pawn.needs.TryGetNeed<Need_Food>();
            if (need == null || (double)need.CurLevel >= lowFloatOfNeeds)
                return;
            pawn.needs.food.CurLevel += need.MaxLevel / 5f;
        }

        public void SatisfyJoy()
        {
            Need_Food need = pawn.needs.TryGetNeed<Need_Food>();
            if (need == null || (double)need.CurLevel >= lowFloatOfNeeds)
                return;
            pawn.needs.food.CurLevel += need.MaxLevel / 5f;
        }

        public void SatisfyThirst()
        {
            if (!SettingPatch.DubsBadHygieneThirstIsActive)
                return;
            Need need = pawn.needs.AllNeeds.Find(x => x.def == SettingPatch.DBHThirstNeed);
            if (need == null || (double)need.CurLevel >= lowFloatOfNeeds)
                return;
            float num = need.MaxLevel / 5f;
            pawn.needs.TryGetNeed(need.def).CurLevel += num;
        }

        public void SatisfyBladder()
        {
            if (!SettingPatch.DubsBadHygieneIsActive)
                return;
            Need need = pawn.needs.AllNeeds.Find(x => x.def == SettingPatch.HygieneNeed);
            if (need == null || (double)need.CurLevel >= lowFloatOfNeeds)
                return;
            float num = need.MaxLevel / 5f;
            pawn.needs.TryGetNeed(need.def).CurLevel += num;
        }

        public void SatisfyHygiene()
        {
            if (!SettingPatch.DubsBadHygieneIsActive)
                return;
            Need need = pawn.needs.AllNeeds.Find(x => x.def == SettingPatch.BladderNeed);
            if (need == null || (double)need.CurLevel >= lowFloatOfNeeds)
                return;
            float num = need.MaxLevel / 5f;
            pawn.needs.TryGetNeed(need.def).CurLevel += num;
        }

        public void SetNextTick() => tickNext = Find.TickManager.TicksGame + 1000;

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