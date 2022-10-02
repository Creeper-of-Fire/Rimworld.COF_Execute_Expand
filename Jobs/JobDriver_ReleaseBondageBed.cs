using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace COF_Torture.Jobs
{
    public class JobDriver_ReleaseBondageBed : JobDriver_UseItem
    {
        protected Thing Thing => this.job.GetTarget(TargetIndex.A).Thing;

        protected Thing Target => this.job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => this.pawn.Reserve((LocalTargetInfo) this.Target, this.job, errorOnFailed: errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            JobDriver_ReleaseBondageBed f = this;
            f.FailOnDestroyedOrNull<JobDriver_ReleaseBondageBed>(TargetIndex.A);
            f.FailOnDestroyedOrNull<JobDriver_ReleaseBondageBed>(TargetIndex.B);
            f.FailOnDespawnedNullOrForbidden<JobDriver_ReleaseBondageBed>(TargetIndex.A);
            f.FailOnAggroMentalStateAndHostile<JobDriver_ReleaseBondageBed>(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnForbidden<Toil>(TargetIndex.A);
            Pawn prisoner = (Pawn) f.Target;
            if (!prisoner.Dead)
            {
                yield return Toils_General.WaitWith(TargetIndex.A, 60, true, true);
                yield return Toils_Reserve.Release(TargetIndex.B);
                yield return JobDriver_ReleaseBondageBed.ReleaseVictim((Building_Bed)this.Thing, (Pawn)this.Target);
            }
        }
        public static Toil ReleaseVictim(Building_Bed Thing, Pawn Target) =>new Toil()
        {
            initAction = (Action) (() =>
            {
                Things.Building_TortureBed thing = (Things.Building_TortureBed)Thing;
                thing.RemoveVictim();
                MoteMaker.ThrowText(Target.PositionHeld.ToVector3(), Target.MapHeld, (string) "CT_Release".Translate(), 4f);
            }),
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}