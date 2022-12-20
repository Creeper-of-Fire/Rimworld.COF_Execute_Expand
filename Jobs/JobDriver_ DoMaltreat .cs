using System.Collections.Generic;
using COF_Torture.Hediffs;
using COF_Torture.Things;
using COF_Torture.Utility;
using RimWorld;
using Verse;
using Verse.AI;

namespace COF_Torture.Jobs
{
    public class JobDriver_DoMaltreat : JobDriver
    {
        public ITortureThing Thing => job.GetTarget(TargetIndex.A).Thing as ITortureThing;

        public Pawn abuser => job.GetTarget(TargetIndex.B).Pawn;
        public Pawn victim => Thing.victim;

        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            Thing == null || victim == null;


        protected override IEnumerable<Toil> MakeNewToils()
        {
            //yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnForbidden(TargetIndex.A);
            var hediffs = victim.health.hediffSet.GetFirstHediff<Hediff_COF_Torture_IsAbusing>();
            var toil = Toils_General.WaitWith(TargetIndex.A, hediffs.Count * 2500 + 200000);
            toil.AddFailCondition(() => hediffs.Count <= 0);
            yield return toil;
        }
    }
}