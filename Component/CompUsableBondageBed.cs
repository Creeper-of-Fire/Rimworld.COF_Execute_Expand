using System.Collections.Generic;
using COF_Torture.Data;
using COF_Torture.Things;
using COF_Torture.Utility;
using RimWorld;
using Verse;
using Verse.AI;
using JobDefOf = COF_Torture.Jobs.JobDefOf;

namespace COF_Torture.Component
{
    public class CompUsableBondageBed : CompUsable
    {
        protected override string FloatMenuOptionLabel(Pawn pawn) => "CT_CantUse".Translate();

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(
            Pawn pawn)
        {
            //var targetA = IsValidBedFor(this.parent,pawn, pawn, true, guestStatus: pawn.GuestStatus);
            //var targetB = RestUtility.IsValidBedFor(this.parent,pawn, pawn, true, guestStatus: pawn.GuestStatus);
            //Log.Message("A:"+targetA + "B:"+targetB);
            CompUsableBondageBed usableBondageBed = this;
            Building_TortureBed ParentBed = usableBondageBed.parent as Building_TortureBed;
            if (ParentBed != null && pawn.Map != null && pawn.Map == Find.CurrentMap)
            {
                if (!pawn.CanReach((LocalTargetInfo)(Thing)usableBondageBed.parent, PathEndMode.Touch,
                        Danger.Deadly))
                    yield return new FloatMenuOption(
                        usableBondageBed.FloatMenuOptionLabel(pawn) + " (" + "NoPath".Translate() + ")",
                        null, MenuOptionPriority.DisabledOption);
                else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                    yield return new FloatMenuOption(
                        usableBondageBed.FloatMenuOptionLabel(pawn) + " (" + "Incapable".Translate() + ")",
                        null, MenuOptionPriority.DisabledOption);
                else if (pawn.WorkTagIsDisabled(WorkTypeDefOf.Warden.workTags))
                    yield return new FloatMenuOption(
                        usableBondageBed.FloatMenuOptionLabel(pawn) + " (" + "CT_Forbid".Translate() + ")",
                        null, MenuOptionPriority.DisabledOption);
                else if (ParentBed.inExecuteProgress)
                {
                    //不可用，则执行释放逻辑
                    if (!pawn.CanReserve((LocalTargetInfo)(Thing)ParentBed)&&!pawn.CanReserve((LocalTargetInfo)(Thing)pawn))
                        yield return new FloatMenuOption(
                            usableBondageBed.FloatMenuOptionLabel(pawn) + " (" +
                            "CT_Reserved".Translate() + ")", null, MenuOptionPriority.DisabledOption);
                    else
                        yield return new FloatMenuOption(
                            "CT_Release_BondageBed".Translate((NamedArgument)ParentBed.victim.Label),
                            ReleaseAction, MenuOptionPriority.GoHere);
                }
                else
                {
                    bool hasColonist = false;
                    foreach (Pawn allPawn in pawn.Map.mapPawns.AllPawns)
                    {
                        Pawn victim = allPawn;
                        if (victim.Spawned && (victim.IsColonist||victim.IsPrisonerOfColony||victim.IsSlaveOfColony))
                        {
                            hasColonist = true;
                            if (victim != pawn)
                            {
                                if (ParentBed.ForPrisoners == victim.IsPrisoner)
                                {
                                    //只有囚犯和囚犯床匹配，或者非囚犯和非囚犯床匹配的才会显示
                                    if (!pawn.CanReserve((LocalTargetInfo)(Thing)victim))
                                        //对象被占用了
                                        yield return new FloatMenuOption(
                                            usableBondageBed.FloatMenuOptionLabel(victim) + " (" +
                                            "CT_Reserved".Translate((NamedArgument)victim.Label) + ")",
                                            null, MenuOptionPriority.DisabledOption);
                                    else if (victim.GetPawnData().IsFixed) //已经被固定
                                    {
                                        //因为已经被固定所以需要先释放
                                        yield return new FloatMenuOption(
                                            "CT_ReleaseAndBound".Translate(pawn.Named(pawn.Name.ToString()),
                                                victim.Named(victim.Name.ToString())),
                                            ReleaseAndBoundAction,
                                            MenuOptionPriority.VeryLow);
                                    }
                                    else
                                    {
                                        //执行捆绑别人
                                        yield return new FloatMenuOption(
                                            "CT_BondageBed".Translate(pawn.Named(pawn.Name.ToString()),
                                                victim.Named(victim.Name.ToString())), BondAction,
                                            MenuOptionPriority.VeryLow);
                                    }
                                }
                            }
                            else if (!ParentBed.ForPrisoners)
                            {
                                //执行捆绑自己
                                hasColonist = true;
                                yield return new FloatMenuOption(
                                    "CT_BondageBedSelf".Translate(),
                                    BondAction,
                                    MenuOptionPriority.GoHere);
                            }

                            void ReleaseAndBoundAction() =>
                                TryReleaseAndBoundVictim(pawn, (LocalTargetInfo)(Thing)victim);

                            void BondAction() => TryStartUseJob(pawn, (LocalTargetInfo)(Thing)victim);
                        }
                    }

                    if (!hasColonist)
                        yield return new FloatMenuOption(
                            usableBondageBed.FloatMenuOptionLabel(pawn) + " (" + "CT_NoTarget".Translate() +
                            ")", null, MenuOptionPriority.DisabledOption);
                }
            }


            void ReleaseAction() => TryReleaseVictim(pawn);
        }

        public virtual void TryReleaseAndBoundVictim(Pawn pawn, LocalTargetInfo extraTarget)
        {
            TryStartUseJob(pawn, extraTarget);
        }

        public virtual void TryReleaseVictim(Pawn pawn)
        {
            if (!pawn.CanReach((LocalTargetInfo)(Thing)parent, PathEndMode.Touch, Danger.Some))
                return;
            Job job = JobMaker.MakeJob(JobDefOf.ReleaseBondageBed,
                (LocalTargetInfo)(Thing)parent);
            job.count = 1;
            pawn.jobs.TryTakeOrderedJob(job);
        }


        public override void TryStartUseJob(Pawn pawn, LocalTargetInfo extraTarget, bool forced = false)
        {
            if (!pawn.CanReserveAndReach((LocalTargetInfo)(Thing)parent, PathEndMode.Touch, Danger.Some))
                return;
            Job job;
            if (pawn == extraTarget)
            {
                job = JobMaker.MakeJob(JobDefOf.UseBondageAlone,
                    (LocalTargetInfo)(Thing)parent);
            }
            else
            {
                if (!pawn.CanReserveAndReach(extraTarget, PathEndMode.Touch, Danger.Some))
                    return;
                job = JobMaker.MakeJob(JobDefOf.UseBondageBed,
                    (LocalTargetInfo)(Thing)parent, extraTarget);
            }

            job.count = 1;
            pawn.jobs.TryTakeOrderedJob(job);
        }
    }
}