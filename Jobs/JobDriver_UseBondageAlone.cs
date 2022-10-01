using System;
using System.Collections.Generic;
using COF_Torture.Things;
using RimWorld;
using Verse;
using Verse.AI;

namespace COF_Torture.Jobs
{
    public class JobDriver_UseBondageAlone : JobDriver_UseItem
    {
        private Building_TortureBed Thing => (Building_TortureBed)this.job.GetTarget(TargetIndex.A).Thing; //building

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(Thing, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Pawn target_pawn = this.pawn as Pawn;
            if (target_pawn == null)
            {
                yield break;
            }

            if (target_pawn.Dead)
            {
                yield break;
            }

            this.FailOnDestroyedOrNull(TargetIndex.A);
            if (this.Thing.victim != this.pawn)
            {
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnForbidden(TargetIndex.A);
                yield return Toils_General.WaitWith(TargetIndex.A, 60, true, true);
                yield return Toils_Reserve.Release(TargetIndex.A);
                yield return JobDriver_UseBondageAlone.BondageIntoBedAlone((Building_Bed)this.Thing, target_pawn);
            }
            /*else if (this.Thing.victim != null)
            {
                yield return JobDriver_UseBondageAlone.BugFixTeleportToBed((Building_Bed) this.Thing, target_pawn);
                yield return JobDriver_UseBondageAlone.BugFixBondageIntoBed((Building_Bed) this.Thing, target_pawn);
            }*/
        }

        public static Toil BondageIntoBedAlone(Building_Bed bed, Pawn takee) => new Toil()
        {
            initAction = (Action)(() =>
            {
                if (bed.Destroyed)
                {
                    takee.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                else
                {
                    Building_TortureBed thing = (Building_TortureBed)bed;
                    thing.SetVictim(takee);
                }

                if (!bed.Destroyed && (bed.OwnersForReading.Contains(takee) ||
                                       bed.Medical && bed.AnyUnoccupiedSleepingSlot || takee.ownership == null))
                {
                    takee.jobs.Notify_TuckedIntoBed(bed);
                    takee.mindState.Notify_TuckedIntoBed();
                }

                if (!takee.IsPrisonerOfColony)
                    return;
                LessonAutoActivator.TeachOpportunity(ConceptDefOf.PrisonerTab, (Thing)takee,
                    OpportunityType.GoodToKnow);
            }),
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        /*
        public static Toil BugFixTeleportToBed(Building_Bed bed, Pawn bugPawn) => new Toil()
        {
            initAction = (Action) (() =>
            {
                if (bed.Destroyed)
                {
                    bugPawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                bugPawn.Position = bed.Position;
            }),
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        
        public static Toil BugFixBondageIntoBed(Building_Bed bed, Pawn takee) => new Toil()
        {
            initAction = (Action) (() =>
            {
                if (bed.Destroyed)
                {
                    takee.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                else
                {
                    Building_TortureBed thing = (Building_TortureBed)bed;
                    thing.SetVictim(takee);
                }
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
        };*/
    }
}