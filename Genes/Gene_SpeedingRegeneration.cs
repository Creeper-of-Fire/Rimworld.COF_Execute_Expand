using System.Collections.Generic;
using Verse;

namespace COF_Torture.Genes
{
    public class Gene_SpeedingRegeneration : Gene
    {
        private int ticksToHeal;

        private IEnumerable<Hediff> hediffsToHeal;
        private static readonly int HealingIntervalTicksRange = 720;

        public override void PostAdd()
        {
            base.PostAdd();
            ResetInterval();
        }

        public override void Tick()
        {
            base.Tick();
            //Log.Message("1");
            --ticksToHeal;
            if (ticksToHeal > 0)
                return;
            ResetInterval();
            //Log.Message("2");
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
                h.Heal(1);
            }
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