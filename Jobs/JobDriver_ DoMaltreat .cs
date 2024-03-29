using System;
using System.Collections.Generic;
using COF_Torture.Data;
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

        private Hediff_COF_Torture_IsAbusing _hediff;

        private Hediff_COF_Torture_IsAbusing Hediff
        {
            get
            {
                if (_hediff == null)
                    _hediff = Hediff_COF_Torture_IsAbusing.AddHediff_COF_Torture_IsAbusing(victim);
                return _hediff;
            }
        }

        public Pawn abuser => pawn;//job.GetTarget(TargetIndex.B).Pawn;
        public Pawn victim => Thing.victim;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return Thing != null && victim != null && !Hediff.ActionList.NullOrEmpty();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(Hediff.ActionList.NullOrEmpty);
            this.FailOn(() => abuser.Downed);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnForbidden(TargetIndex.A);
            yield return Toils_General.WaitWith(TargetIndex.A, 60, true, true);
            yield return StartHediffTick();
            //var hediffs = victim.health.hediffSet.GetFirstHediff<Hediff_COF_Torture_IsAbusing>();
            var ticks = Hediff.Count * 2500 + 200000;
            ModLog.Message(ticks.ToString());
            var toil = Toils_General.WaitWith(TargetIndex.A, ticks);
            toil.AddFailCondition(() => Hediff.Count <= 0);
            yield return toil.FailOn(Hediff.ActionList.NullOrEmpty);
        }

        private Toil StartHediffTick()
        {
            var toil = ToilMaker.MakeToil(nameof(StartHediffTick));
            toil.initAction = delegate
            {
                Hediff.Start();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }
    }
}