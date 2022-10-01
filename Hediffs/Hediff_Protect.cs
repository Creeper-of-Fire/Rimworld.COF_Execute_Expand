using System;
using System.Collections.Generic;
using System.Linq;
using COF_Torture.ModSetting;
using COF_Torture.Patch;
using RimWorld;
using Verse;

namespace COF_Torture.Hediffs
{
    public class Hediff_Protect : Hediff_Torture
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
            if (giver == null)
            {
                this.Severity = 0.0f;
            }
            if (ModSettingMain.Instance.Setting.isFeed)
            {
                this.SatisfyHunger();
                this.SatisfyThirst();
            }
            this.SetNextTick();
        }

        /*public void HealWounds()
        {
          IEnumerable<Hediff> hediffs = this.pawn.health.hediffSet.hediffs.Where<Hediff>((Func<Hediff, bool>) (hd => !hd.IsTended() && hd.TendableNow()));
          if (hediffs == null)
            return;
          foreach (Hediff hediff in hediffs)
          {
            if (hediff is HediffWithComps hd)
            {
              if (hd.Bleeding)
              {
                hd.Heal(0.5f);
              }
              else
              {
                if (hd.def.chronic || (double) hd.def.lethalSeverity <= 0.0)
                {
                  HediffStage curStage = hd.CurStage;
                  if ((curStage != null ? (curStage.lifeThreatening ? 1 : 0) : 0) == 0)
                    continue;
                }
                HediffComp_TendDuration comp = hd.TryGetComp<HediffComp_TendDuration>();
                comp.tendQuality = 1f;
                comp.tendTicksLeft = 10000;
                this.pawn.health.Notify_HediffChanged(hediff);
              }
            }
          }
        }*/

        public void SatisfyHunger()
        {
            Need_Food need = this.pawn.needs.TryGetNeed<Need_Food>();
            if (need == null || (double)need.CurLevel >= 0.15000000596046448)
                return;
            this.pawn.needs.food.CurLevel += need.MaxLevel / 5f;
        }

        public void SatisfyThirst()
        {
            if (!SettingPatch.DubsBadHygieneIsActive)
                return;
            Need need = this.pawn.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == SettingPatch.DBHThirst));
            if (need == null || (double)need.CurLevel >= 0.15000000596046448)
                return;
            float num = need.MaxLevel / 5f;
            this.pawn.needs.TryGetNeed(need.def).CurLevel += num;
        }

        public void SetNextTick() => this.tickNext = Find.TickManager.TicksGame + 1000;
    }
}