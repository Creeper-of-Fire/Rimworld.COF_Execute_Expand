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
            if (this.Thing.GetVictim() != this.pawn)
            {
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnForbidden(TargetIndex.A);
                yield return Toils_General.WaitWith(TargetIndex.A, 60, true, true);
                yield return Toils_Reserve.Release(TargetIndex.A);
                yield return CT_Toils_GoToBed.BondageIntoBed((Building_Bed)this.Thing, target_pawn);
            }
            /*else if (this.Thing.victim != null)
            {
                yield return JobDriver_UseBondageAlone.BugFixTeleportToBed((Building_Bed) this.Thing, target_pawn);
                yield return JobDriver_UseBondageAlone.BugFixBondageIntoBed((Building_Bed) this.Thing, target_pawn);
            }*/
        }
    }
}