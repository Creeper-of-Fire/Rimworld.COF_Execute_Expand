using System.Collections.Generic;
using COF_Torture.Things;
using RimWorld;
using Verse;
using Verse.AI;

namespace COF_Torture.Jobs
{
    public class JobDriver_ReleaseBondageBed : JobDriver_UseItem
    {
        protected Thing Thing => job.GetTarget(TargetIndex.A).Thing;

        //protected Thing Target => this.job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;
        //this.pawn.Reserve((LocalTargetInfo)this.Thing, this.job, errorOnFailed: errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            JobDriver_ReleaseBondageBed f = this;
            f.FailOnDestroyedOrNull(TargetIndex.A);
            //f.FailOnDestroyedOrNull<JobDriver_ReleaseBondageBed>(TargetIndex.B);
            f.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            //f.FailOnAggroMentalStateAndHostile<JobDriver_ReleaseBondageBed>(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                .FailOnForbidden(TargetIndex.A);
            //Pawn prisoner = (Pawn) f.Target;
            yield return Toils_General.WaitWith(TargetIndex.A, 60, true, true);
            //yield return Toils_Reserve.Release(TargetIndex.B);
            yield return ReleaseVictim((Building_Bed)Thing);
        }

        /// <summary>
        /// 释放处刑对象
        /// </summary>
        public static Toil ReleaseVictim(Building_Bed Thing)
        {
            var toil = ToilMaker.MakeToil();
            toil.initAction = () =>
            {
                Building_TortureBed thing = (Building_TortureBed)Thing;
                thing.ReleaseVictim();
                MoteMaker.ThrowText(Thing.PositionHeld.ToVector3(), Thing.MapHeld, "CT_Release".Translate(),
                    4f);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }
    }
}