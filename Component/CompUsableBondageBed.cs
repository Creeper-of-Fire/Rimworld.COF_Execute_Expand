using System;
using System.Collections.Generic;
using System.Linq;
using COF_Torture.Things;
using RimWorld;
using Verse;
using Verse.AI;

namespace COF_Torture.Component
{
    public class CompUsableBondageBed : CompUsable
    {
        protected override string FloatMenuOptionLabel(Pawn pawn) => (string)"SR_CantUse".Translate();


/*
        public static bool IsValidBedFor(
            Thing bedThing,
            Pawn sleeper,
            Pawn traveler,
            bool checkSocialProperness,
            bool allowMedBedEvenIfSetToNoCare = false,
            bool ignoreOtherReservations = false,
            GuestStatus? guestStatus = null)
        {
            if (!(bedThing is Building_Bed buildingBed) ||
                !traveler.CanReserveAndReach((LocalTargetInfo)(Thing)buildingBed, PathEndMode.OnCell, Danger.Some,
                    buildingBed.SleepingSlotsCount, ignoreOtherReservations: ignoreOtherReservations) ||
                traveler.HasReserved<JobDriver_TakeToBed>((LocalTargetInfo)(Thing)buildingBed,
                    new LocalTargetInfo?((LocalTargetInfo)(Thing)sleeper)) ||
                !RestUtility.CanUseBedEver(sleeper, buildingBed.def) || !buildingBed.AnyUnoccupiedSleepingSlot &&
                (!sleeper.InBed() || sleeper.CurrentBed() != buildingBed) &&
                !buildingBed.CompAssignableToPawn.AssignedPawns.Contains<Pawn>(sleeper) ||
                buildingBed.IsForbidden(traveler))
                return false;
            GuestStatus? nullable = guestStatus;
            GuestStatus guestStatus1 = GuestStatus.Prisoner;
            bool forPrisoner = nullable.GetValueOrDefault() == guestStatus1 & nullable.HasValue;
            nullable = guestStatus;
            GuestStatus guestStatus2 = GuestStatus.Slave;
            bool flag = nullable.GetValueOrDefault() == guestStatus2 & nullable.HasValue;
            if (checkSocialProperness && !buildingBed.IsSociallyProper(sleeper, forPrisoner) ||
                buildingBed.CompAssignableToPawn.IdeoligionForbids(sleeper) || buildingBed.IsBurning())
                return false;
            if (forPrisoner)
            {
                if (!buildingBed.ForPrisoners || !buildingBed.Position.IsInPrisonCell(buildingBed.Map))
                    return false;
            }
            else if (flag)
            {
                if (!buildingBed.ForSlaves)
                    return false;
            }
            else if (buildingBed.Faction != traveler.Faction &&
                     (traveler.HostFaction == null || buildingBed.Faction != traveler.HostFaction) ||
                     buildingBed.ForPrisoners || buildingBed.ForSlaves)
                return false;

            if (buildingBed.Medical)
            {
                if (!allowMedBedEvenIfSetToNoCare && !HealthAIUtility.ShouldEverReceiveMedicalCareFromPlayer(sleeper) ||
                    !HealthAIUtility.ShouldSeekMedicalRest(sleeper))
                    return false;
            }
            else if (buildingBed.OwnersForReading.Any<Pawn>() && !buildingBed.OwnersForReading.Contains(sleeper))
            {
                if (((sleeper.IsPrisoner | forPrisoner ? 1 : (sleeper.IsSlave ? 1 : 0)) | (flag ? 1 : 0)) != 0)
                {
                    if (!buildingBed.AnyUnownedSleepingSlot)
                        return false;
                }
                else if (!IsAnyOwnerLovePartnerOf(buildingBed, sleeper) || !buildingBed.AnyUnownedSleepingSlot)
                    return false;
            }

            return true;
        }

        private static bool IsAnyOwnerLovePartnerOf(Building_Bed bed, Pawn sleeper)
        {
            for (int index = 0; index < bed.OwnersForReading.Count; ++index)
            {
                if (LovePartnerRelationUtility.LovePartnerRelationExists(sleeper, bed.OwnersForReading[index]))
                    return true;
            }

            return false;
        }
*/

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(
            Pawn pawn)
        {
            //var targetA = IsValidBedFor(this.parent,pawn, pawn, true, guestStatus: pawn.GuestStatus);
            //var targetB = RestUtility.IsValidBedFor(this.parent,pawn, pawn, true, guestStatus: pawn.GuestStatus);
            //Log.Message("A:"+targetA + "B:"+targetB);
            CompUsableBondageBed usableBondageBed = this;
            Building_TortureBed bbb = usableBondageBed.parent as Building_TortureBed;
            if (bbb != null && pawn.Map != null && pawn.Map == Find.CurrentMap)
            {
                if (!pawn.CanReach((LocalTargetInfo)(Verse.Thing)usableBondageBed.parent, PathEndMode.Touch,
                        Danger.Deadly))
                    yield return new FloatMenuOption(
                        (string)(usableBondageBed.FloatMenuOptionLabel(pawn) + " (" + "NoPath".Translate() + ")"),
                        (Action)null, MenuOptionPriority.DisabledOption);
                else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                    yield return new FloatMenuOption(
                        (string)(usableBondageBed.FloatMenuOptionLabel(pawn) + " (" + "Incapable".Translate() + ")"),
                        (Action)null, MenuOptionPriority.DisabledOption);
                else if (pawn.WorkTagIsDisabled(WorkTypeDefOf.Warden.workTags))
                    yield return new FloatMenuOption(
                        (string)(usableBondageBed.FloatMenuOptionLabel(pawn) + " (" + "CT_Forbid".Translate() + ")"),
                        (Action)null, MenuOptionPriority.DisabledOption);
                else if (bbb.victim != null)
                {
                    //执行释放
                    if (!pawn.CanReserve((LocalTargetInfo)(Verse.Thing)bbb.victim))
                        yield return new FloatMenuOption(
                            (string)(usableBondageBed.FloatMenuOptionLabel(bbb.victim) + " (" +
                                     "CT_Reserved".Translate() + ")"), (Action)null, MenuOptionPriority.DisabledOption);
                    else
                        yield return new FloatMenuOption(
                            (string)"CT_Release_BondageBed".Translate((NamedArgument)bbb.victim.Label),
                            new Action(ReleaseAction), MenuOptionPriority.DisabledOption);
                }
                else
                {
                    bool hasColonist = false;
                    foreach (Pawn allPawn in pawn.Map.mapPawns.AllPawns)
                    {
                        Pawn prisoner = allPawn;

                        if (prisoner != pawn && prisoner.Spawned && prisoner.IsColonist)
                        {
                            //执行捆绑别人
                            hasColonist = true;
                            if (!pawn.CanReserve((LocalTargetInfo)(Verse.Thing)prisoner))
                                yield return new FloatMenuOption(
                                    (string)(usableBondageBed.FloatMenuOptionLabel(prisoner) + " (" +
                                             "CT_Reserved".Translate((NamedArgument)prisoner.Label) + ")"),
                                    (Action)null, MenuOptionPriority.DisabledOption);
                            else if (prisoner.health.hediffSet.HasHediff(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Fixed))
                            {
                                //因为已经被固定所以需要释放
                                yield return new FloatMenuOption(
                                    (string)"CT_ReleaseAndBound".Translate(pawn.Named(pawn.Name.ToString()),
                                        prisoner.Named(prisoner.Name.ToString())), new Action(ReleaseAndBoundAction),
                                    MenuOptionPriority.DisabledOption);
                            }
                            else
                            {
                                yield return new FloatMenuOption(
                                    (string)"CT_BondageBed".Translate(pawn.Named(pawn.Name.ToString()),
                                        prisoner.Named(prisoner.Name.ToString())), new Action(BondAction),
                                    MenuOptionPriority.DisabledOption);
                            }
                        }
                        else if (prisoner.Spawned && prisoner.IsColonist)
                        {
                            //执行捆绑自己
                            hasColonist = true;
                            yield return new FloatMenuOption(
                                (string)"CT_BondageBedSelf".Translate(),
                                new Action(BondAction),
                                MenuOptionPriority.DisabledOption);
                        }

                        void ReleaseAndBoundAction() =>
                            this.TryReleaseAndBoundVictim(pawn, (LocalTargetInfo)(Verse.Thing)prisoner);
                        void BondAction() => this.TryStartUseJob(pawn, (LocalTargetInfo)(Verse.Thing)prisoner);
                    }

                    if (!hasColonist)
                        yield return new FloatMenuOption(
                            (string)(usableBondageBed.FloatMenuOptionLabel(pawn) + " (" + "CT_NoTarget".Translate() +
                                     ")"), (Action)null, MenuOptionPriority.DisabledOption);
                }
            }

            void ReleaseAction() => this.TryReleaseVictim(pawn, (LocalTargetInfo)(Verse.Thing)bbb.victim);
        }

        public virtual void TryReleaseAndBoundVictim(Pawn pawn, LocalTargetInfo extraTarget)
        {
            this.TryStartUseJob(pawn, extraTarget);
        }

        public virtual void TryReleaseVictim(Pawn pawn, LocalTargetInfo extraTarget)
        {
            if (!pawn.CanReach((LocalTargetInfo)(Verse.Thing)this.parent, PathEndMode.Touch, Danger.Some) ||
                !pawn.CanReserveAndReach(extraTarget, PathEndMode.Touch, Danger.Some))
                return;
            Job job = JobMaker.MakeJob(COF_Torture.Jobs.JobDefOf.ReleaseBondageBed,
                (LocalTargetInfo)(Verse.Thing)this.parent, extraTarget);
            job.count = 1;
            pawn.jobs.TryTakeOrderedJob(job);
        }


        public override void TryStartUseJob(Pawn pawn, LocalTargetInfo extraTarget)
        {
            if (!pawn.CanReserveAndReach((LocalTargetInfo)(Verse.Thing)this.parent, PathEndMode.Touch, Danger.Some))
                return;
            Job job;
            if (pawn == extraTarget)
            {
                job = JobMaker.MakeJob(COF_Torture.Jobs.JobDefOf.UseBondageAlone,
                    (LocalTargetInfo)(Verse.Thing)this.parent);
            }
            else
            {
                if (!pawn.CanReserveAndReach(extraTarget, PathEndMode.Touch, Danger.Some))
                    return;
                job = JobMaker.MakeJob(COF_Torture.Jobs.JobDefOf.UseBondageBed,
                    (LocalTargetInfo)(Verse.Thing)this.parent, extraTarget);
            }
            job.count = 1;
            pawn.jobs.TryTakeOrderedJob(job);
        }
    }
}