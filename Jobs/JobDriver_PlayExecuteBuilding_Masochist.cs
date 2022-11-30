using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace COF_Torture.Jobs
{
    public class JobDriver_PlayExecuteBuilding_Masochist : JobDriver
    {
        //private const TargetIndex BuildingToPlay = TargetIndex.A;
        
        private Building BuildingToPlay => (Building) TargetA;

        //private Toil beingFuck;
        
        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            pawn.Reserve(job.targetA,
                job,
                job.def.joyMaxParticipants,
                0);
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return new Toil().FailOnDestroyedNullOrForbidden(TargetIndex.A);
            if (pawn.story.traits.HasTrait(TraitDefOf.Masochist) && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
            {
                if (pawn.Position != TargetA.Cell)
                    yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
                yield return BeingFuck();
            }
        }

        private Toil BeingFuck()
        {
            Toil beingFuck = ToilMaker.MakeToil();
            beingFuck.defaultDuration = 3000;
            beingFuck.defaultCompleteMode = ToilCompleteMode.Delay;
            beingFuck.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            beingFuck.initAction = () =>
            {
                //this.Map.mapDrawer.MapMeshDirty(this.sexBuilding.Position, MapMeshFlag.Buildings);
                //beingFuck.actor.pather.StopDead();
                pawn.jobs.posture = PawnPosture.LayingOnGroundFaceUp;
            };
            //int tickCounter = 0;
            beingFuck.tickAction = () =>
            { 
                Pawn actor = beingFuck.actor;
                Job curJob = actor.CurJob;
                JobDriver curDriver = actor.jobs.curDriver;
                //actor.Drawer.renderer.graphics.ClearCache();
                //actor.Drawer.renderer.graphics.apparelGraphics.Clear();
                actor.GainComfortFromCellIfPossible();
                JoyUtility.JoyTickCheckEnd(pawn, joySource: ((Building) TargetA));
            };
            return beingFuck;
        }
    }
}