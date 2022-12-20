using System;
using System.Collections.Generic;
using COF_Torture.Dialog;
using COF_Torture.Hediffs;
using COF_Torture.Things;
using COF_Torture.Utility;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace COF_Torture.Jobs
{
    /*public class JobDriver_GotoAndOpenWindow : JobDriver
    {
        private ITortureThing Thing => (ITortureThing)(Thing)TargetA;

        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            Thing == null;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnForbidden(TargetIndex.A);
            yield return CT_Toils_GoToBed.OpenAWindow(new Dialog_AbuseMenu(Thing.victim, Thing, pawn));
            /*Toil toil = ToilMaker.MakeToil();
            toil.initAction = () =>
            {
                if (this.pawn.mindState != null && this.pawn.mindState.forcedGotoPosition == this.TargetA.Cell)
                    this.pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
                if (!this.job.ritualTag.NullOrEmpty() && this.pawn.GetLord()?.LordJob is LordJob_Ritual lordJob2)
                    lordJob2.AddTagForPawn(this.pawn, this.job.ritualTag);
                if (!this.job.exitMapOnArrival || !this.pawn.Position.OnEdge(this.pawn.Map) &&
                    !this.pawn.Map.exitMapGrid.IsExitCell(this.pawn.Position))
                    return;
                this.TryExitMap();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
        }

        private void TryExitMap()
        {
            if (this.job.failIfCantJoinOrCreateCaravan &&
                !CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(this.pawn))
                return;
            if (ModsConfig.BiotechActive)
                MechanitorUtility.Notify_PawnGotoLeftMap(this.pawn, this.pawn.Map);
            this.pawn.ExitMap(true, CellRect.WholeMap(this.Map).GetClosestEdge(this.pawn.Position));
        }
    }*/
}