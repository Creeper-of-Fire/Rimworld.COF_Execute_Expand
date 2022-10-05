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

        //protected Thing Target => this.job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;
            //this.pawn.Reserve((LocalTargetInfo)this.Thing, this.job, errorOnFailed: errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            JobDriver_ReleaseBondageBed f = this;
            f.FailOnDestroyedOrNull<JobDriver_ReleaseBondageBed>(TargetIndex.A);
            //f.FailOnDestroyedOrNull<JobDriver_ReleaseBondageBed>(TargetIndex.B);
            f.FailOnDespawnedNullOrForbidden<JobDriver_ReleaseBondageBed>(TargetIndex.A);
            //f.FailOnAggroMentalStateAndHostile<JobDriver_ReleaseBondageBed>(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                .FailOnForbidden<Toil>(TargetIndex.A);
            //Pawn prisoner = (Pawn) f.Target;
            yield return Toils_General.WaitWith(TargetIndex.A, 60, true, true);
            //yield return Toils_Reserve.Release(TargetIndex.B);
            yield return JobDriver_ReleaseBondageBed.ReleaseVictim((Building_Bed)this.Thing);
        }

        public static Toil ReleaseVictim(Building_Bed Thing) => new Toil()
        {
            initAction = (Action)(() =>
            {
                Things.Building_TortureBed thing = (Things.Building_TortureBed)Thing;
                thing.ReleaseContainer();
                MoteMaker.ThrowText(Thing.PositionHeld.ToVector3(), Thing.MapHeld, (string)"CT_Release".Translate(), 4f);
            }),
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}