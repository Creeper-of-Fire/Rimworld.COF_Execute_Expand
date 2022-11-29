using System;
using System.Collections.Generic;
using COF_Torture.Data;
using COF_Torture.Things;
using RimWorld;
using Verse;
using Verse.AI;

namespace COF_Torture.Component
{
    public class CompUsableBondageBed : CompUsable
    {
        protected override string FloatMenuOptionLabel(Pawn pawn) => (string)"CT_CantUse".Translate();

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
                else if (ParentBed.inExecuteProgress)
                {
                    //不可用，则执行释放逻辑
                    if (!pawn.CanReserve((LocalTargetInfo)(Verse.Thing)ParentBed)&&!pawn.CanReserve((LocalTargetInfo)(Verse.Thing)pawn))
                        yield return new FloatMenuOption(
                            (string)(usableBondageBed.FloatMenuOptionLabel(pawn) + " (" +
                                     "CT_Reserved".Translate() + ")"), (Action)null, MenuOptionPriority.DisabledOption);
                    else
                        yield return new FloatMenuOption(
                            (string)"CT_Release_BondageBed".Translate((NamedArgument)ParentBed.victim.Label),
                            new Action(ReleaseAction), MenuOptionPriority.GoHere);
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
                                    if (!pawn.CanReserve((LocalTargetInfo)(Verse.Thing)victim))
                                        //对象被占用了
                                        yield return new FloatMenuOption(
                                            (string)(usableBondageBed.FloatMenuOptionLabel(victim) + " (" +
                                                     "CT_Reserved".Translate((NamedArgument)victim.Label) + ")"),
                                            (Action)null, MenuOptionPriority.DisabledOption);
                                    else if (victim.GetPawnData().IsFixed) //已经被固定
                                    {
                                        //因为已经被固定所以需要先释放
                                        yield return new FloatMenuOption(
                                            (string)"CT_ReleaseAndBound".Translate(pawn.Named(pawn.Name.ToString()),
                                                victim.Named(victim.Name.ToString())),
                                            new Action(ReleaseAndBoundAction),
                                            MenuOptionPriority.VeryLow);
                                    }
                                    else
                                    {
                                        //执行捆绑别人
                                        yield return new FloatMenuOption(
                                            (string)"CT_BondageBed".Translate(pawn.Named(pawn.Name.ToString()),
                                                victim.Named(victim.Name.ToString())), new Action(BondAction),
                                            MenuOptionPriority.VeryLow);
                                    }
                                }
                            }
                            else if (!ParentBed.ForPrisoners)
                            {
                                //执行捆绑自己
                                hasColonist = true;
                                yield return new FloatMenuOption(
                                    (string)"CT_BondageBedSelf".Translate(),
                                    new Action(BondAction),
                                    MenuOptionPriority.GoHere);
                            }

                            void ReleaseAndBoundAction() =>
                                this.TryReleaseAndBoundVictim(pawn, (LocalTargetInfo)(Verse.Thing)victim);

                            void BondAction() => this.TryStartUseJob(pawn, (LocalTargetInfo)(Verse.Thing)victim);
                        }
                    }

                    if (!hasColonist)
                        yield return new FloatMenuOption(
                            (string)(usableBondageBed.FloatMenuOptionLabel(pawn) + " (" + "CT_NoTarget".Translate() +
                                     ")"), (Action)null, MenuOptionPriority.DisabledOption);
                }
            }


            void ReleaseAction() => this.TryReleaseVictim(pawn);
        }

        public virtual void TryReleaseAndBoundVictim(Pawn pawn, LocalTargetInfo extraTarget)
        {
            this.TryStartUseJob(pawn, extraTarget);
        }

        public virtual void TryReleaseVictim(Pawn pawn)
        {
            if (!pawn.CanReach((LocalTargetInfo)(Verse.Thing)this.parent, PathEndMode.Touch, Danger.Some))
                return;
            Job job = JobMaker.MakeJob(COF_Torture.Jobs.JobDefOf.ReleaseBondageBed,
                (LocalTargetInfo)(Verse.Thing)this.parent);
            job.count = 1;
            pawn.jobs.TryTakeOrderedJob(job);
        }


        public override void TryStartUseJob(Pawn pawn, LocalTargetInfo extraTarget, bool forced = false)
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