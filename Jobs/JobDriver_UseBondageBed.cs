using System;
using System.Collections.Generic;
using COF_Torture.Hediffs;
using COF_Torture.Things;
using RimWorld;
using Verse;
using Verse.AI;

namespace COF_Torture.Jobs
{
    public class JobDriver_UseBondageBed: JobDriver_UseItem
    {
        private Thing Thing => this.job.GetTarget(TargetIndex.A).Thing; //building
        private Thing Target => this.job.GetTarget(TargetIndex.B).Thing; //target
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(Thing, job, 1, -1, null, errorOnFailed) &&
                   this.pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnAggroMentalStateAndHostile(TargetIndex.B);
            Pawn target_pawn = Target as Pawn;
            if (target_pawn == null)
            {
                yield break;
            }
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
            Hediff_Protect hediffFixed = (Hediff_Protect)target_pawn.health.hediffSet.GetFirstHediffOfDef(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Fixed);
            if (hediffFixed != null)
            {
                Building_Bed That_bed = (Building_Bed) hediffFixed.giver;
                if (That_bed != null)
                    yield return JobDriver_ReleaseBondageBed.ReleaseVictim(That_bed, target_pawn);
                else
                {
                    Log.Error("[COF_TORTURE]试图解绑一个殖民者，但是其‘捆绑’状态没有给予者");
                    hediffFixed.Severity = 0.0f;
                }
            }
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnForbidden(TargetIndex.A);
            if (target_pawn.Dead)
            {
                yield break;
            }
            yield return Toils_General.WaitWith(TargetIndex.A, 60, true, true);
            yield return Toils_Reserve.Release(TargetIndex.A);
            yield return JobDriver_UseBondageBed.BondageIntoBedByOthers((Building_Bed) this.Thing, this.pawn, target_pawn);
        }
        
        public static Toil BondageIntoBedByOthers(Building_Bed bed, Pawn taker, Pawn takee) => new Toil()
        {
            initAction = (Action) (() =>
            {
                if (bed.Destroyed)
                {
                    taker.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                else
                {
                    Building_TortureBed thing = (Building_TortureBed)bed;
                    thing.SetVictim(takee);
                }
                taker.carryTracker.TryDropCarriedThing(bed.Position, ThingPlaceMode.Direct, out Thing _);
                if (!bed.Destroyed && (bed.OwnersForReading.Contains(takee) || bed.Medical && bed.AnyUnoccupiedSleepingSlot || takee.ownership == null))
                {
                    takee.jobs.Notify_TuckedIntoBed(bed);
                    takee.mindState.Notify_TuckedIntoBed();
                }
                if (!takee.IsPrisonerOfColony)
                    return;
                LessonAutoActivator.TeachOpportunity(ConceptDefOf.PrisonerTab, (Thing) takee, OpportunityType.GoodToKnow);
            }),
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}