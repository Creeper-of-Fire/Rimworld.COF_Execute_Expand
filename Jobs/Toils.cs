using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace COF_Torture.Jobs
{
    public class Toils
    {
        public static Toil GetToilLayDownBondage(
      TargetIndex bedOrRestSpotIndex,
      bool lookForOtherJobs = false,
      bool canSleep = true,
      bool gainRestAndHealth = true)
    {
      Toil layDownBondage = new Toil();
      layDownBondage.initAction = (Action) (() =>
      {
        Pawn actor = layDownBondage.actor;
        actor.pather.StopDead();
        JobDriver curDriver = actor.jobs.curDriver;
        actor.jobs.posture = PawnPosture.LayingInBed;
        actor.mindState.lastBedDefSleptIn = (ThingDef) null;
        curDriver.asleep = false;
      });
      layDownBondage.tickAction = (Action) (() =>
      {
        Pawn actor = layDownBondage.actor;
        Job curJob = actor.CurJob;
        JobDriver curDriver = actor.jobs.curDriver;
        Thing thing = curJob.GetTarget(bedOrRestSpotIndex).Thing;
        actor.GainComfortFromCellIfPossible();
        if (!curDriver.asleep)
        {
          if (canSleep &&
              (actor.needs.rest != null &&
               (double)actor.needs.rest.CurLevel < (double)RestUtility.FallAsleepMaxLevel(actor) ||
               curJob.forceSleep))
            curDriver.asleep = true;
        }
        else if (!canSleep)
          curDriver.asleep = false;
        else if ((actor.needs.rest == null ||
                  (double)actor.needs.rest.CurLevel >= (double)RestUtility.WakeThreshold(actor)) &&
                 !curJob.forceSleep)
          curDriver.asleep = false;
        if (curDriver.asleep & gainRestAndHealth && actor.needs.rest != null)
        {
          float restEffectiveness;
          if (thing == null || !thing.def.statBases.StatListContains(StatDefOf.BedRestEffectiveness))
            restEffectiveness = StatDefOf.BedRestEffectiveness.valueIfMissing;
          else
            restEffectiveness = thing.GetStatValue(StatDefOf.BedRestEffectiveness);
          actor.needs.rest.TickResting(restEffectiveness);
        }
        if (actor.IsHashIntervalTick(100) && !actor.Position.Fogged(actor.Map))
        {
          if (curDriver.asleep)
            FleckMaker.ThrowMetaIcon(actor.Position, actor.Map, FleckDefOf.SleepZ);
          if (gainRestAndHealth && actor.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>())
            FleckMaker.ThrowMetaIcon(actor.Position, actor.Map, FleckDefOf.HealingCross);
        }
        if (!lookForOtherJobs || !actor.IsHashIntervalTick(211))
          return;
        actor.jobs.CheckForJobOverride();
      });
      layDownBondage.defaultCompleteMode = ToilCompleteMode.Never;
      layDownBondage.FailOn<Toil>((Func<bool>) (() => !layDownBondage.actor.health.capacities.CapableOf(PawnCapacityDefOf.Moving)));
      layDownBondage.AddFinishAction((Action) (() => layDownBondage.actor.jobs.curDriver.asleep = false));
      return layDownBondage;
    }
    }
}