using System.Collections.Generic;
using COF_Torture.Hediffs;
using Verse;

namespace COF_Torture.Genes
{
    /// <summary>
    /// 快速恢复，但是会累积快感 TODO 懒得写GeneDef了
    /// </summary>
    public class Gene_SpeedingRegeneration : Gene
    {
        private int ticksToHeal;

        private IEnumerable<Hediff> hediffsToHeal;
        private static readonly int HealingIntervalTicksRange = 720;
        private static readonly float HealingAmount = 1;

        public override void PostAdd()
        {
            base.PostAdd();
            ResetInterval();
        }

        public override void Tick()
        {
            base.Tick();
            --ticksToHeal;
            if (ticksToHeal > 0)
                return;
            ResetInterval();
            if (hediffsToHeal == null)
            {
                hediffsToHeal = SetHealAbleHediffs(pawn.health);
            }

            if (hediffsToHeal != null)
                HealHediffs(pawn.health, hediffsToHeal);
        }

        public static IEnumerable<Hediff> SetHealAbleHediffs(Pawn_HealthTracker health)
        {
            foreach (var h in health.hediffSet.hediffs)
            {
                if (h.def.tendable || h.IsPermanent())
                {
                    yield return h;
                }
                else if (h.def.lethalSeverity > 0)
                {
                    yield return h;
                }
            }
        }

        public static void HealHediffs(Pawn_HealthTracker health, IEnumerable<Hediff> hediffs)
        {
            foreach (var h in hediffs)
            {
                h.Heal(HealingAmount);
            }

            Hediff_COF_Sexual_Gratification.GetHediffOrgasmIndicator(health.hediffSet.pawn)
                .InOrDecrease_Gratification(HealingAmount);
        }

        private void ResetInterval() =>
            ticksToHeal = HealingIntervalTicksRange;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToHeal, "ticksToHeal");
            //Scribe_References.Look<Hediff>(ref this.hediffToHeal, "hediffToHeal");
        }
    }
}