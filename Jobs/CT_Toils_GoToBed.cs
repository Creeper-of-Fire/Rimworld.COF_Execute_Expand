using System;
using COF_Torture.Things;
using RimWorld;
using Verse;
using Verse.AI;

namespace COF_Torture.Jobs
{
    public class CT_Toils_GoToBed
    {
        public static Toil BondageIntoBed(Building_Bed bed, Pawn takee, Pawn taker = null)
        {
            var toil = ToilMaker.MakeToil(nameof(BondageIntoBed));
            toil.initAction = (Action)(() =>
            {
                if (bed.Destroyed)
                {
                    if (taker == null)
                        takee.jobs.EndCurrentJob(JobCondition.Incompletable);
                    else
                    {
                        taker.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }
                }
                else
                {
                    Building_TortureBed thing = (Building_TortureBed)bed;
                    thing.SetVictim(takee);
                }

                if (taker != null)
                    taker.carryTracker.TryDropCarriedThing(bed.Position, ThingPlaceMode.Direct, out Thing _);
                if (!bed.Destroyed && (bed.OwnersForReading.Contains(takee) ||
                                       bed.Medical && bed.AnyUnoccupiedSleepingSlot || takee.ownership == null))
                {
                    CT_TuckedIntoBed(bed, takee);
                    takee.mindState.Notify_TuckedIntoBed();
                }

                if (!takee.IsPrisonerOfColony)
                    return;
                LessonAutoActivator.TeachOpportunity(ConceptDefOf.PrisonerTab, (Thing)takee,
                    OpportunityType.GoodToKnow);
            });
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        public static void BugFixBondageIntoBed(Building_Bed bed, Pawn takee)
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

            if (!bed.Destroyed)
            {
                takee.Position = RestUtility.GetBedSleepingSlotPosFor(takee, bed);
                takee.Notify_Teleported(false);
                takee.stances.CancelBusyStanceHard();
                takee.jobs.StartJob(JobMaker.MakeJob(Jobs.JobDefOf.CT_LayDown, (LocalTargetInfo)(Thing)bed),
                    JobCondition.InterruptForced, tag: new JobTag?(JobTag.TuckedIntoBed));
                takee.mindState.Notify_TuckedIntoBed();
            }

            LessonAutoActivator.TeachOpportunity(ConceptDefOf.PrisonerTab, (Thing)takee, OpportunityType.GoodToKnow);
        }

        public static void CT_TuckedIntoBed(Building_Bed bed, Pawn takee)
        {
            takee.Position = RestUtility.GetBedSleepingSlotPosFor(takee, bed);
            takee.Notify_Teleported(false);
            takee.stances.CancelBusyStanceHard();
            takee.jobs.StartJob(JobMaker.MakeJob(JobDefOf.CT_LayDown, (LocalTargetInfo)(Thing)bed),
                JobCondition.InterruptForced, tag: new JobTag?(JobTag.TuckedIntoBed));
        }
    }
}