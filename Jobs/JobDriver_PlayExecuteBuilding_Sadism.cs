using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace COF_Torture.Jobs
{
    /*public class JobDriver_PlayExecuteBuilding_Sadism : JobDriver
    {
        //private const TargetIndex BuildingToPlay = TargetIndex.A;
        
        private Building BuildingToPlay => (Building) this.TargetA;

        //private Toil beingFuck;
        
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return new Toil().FailOnDestroyedNullOrForbidden<Toil>(TargetIndex.A);
            if (this.pawn.story.traits.HasTrait(TraitDefOf.Masochist) && this.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
            {
                if (this.pawn.Position != this.TargetA.Cell)
                    yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.ClosestTouch);
                yield return BeingFuck();
            }
        }

        private Toil BeingFuck()
        {
            Toil beingFuck = ToilMaker.MakeToil(nameof(BeingFuck));
            beingFuck.defaultDuration = 3000;
            beingFuck.defaultCompleteMode = ToilCompleteMode.Delay;
            beingFuck.FailOnDestroyedNullOrForbidden<Toil>(TargetIndex.A);
            beingFuck.initAction = (Action) (() =>
            {
                //this.Map.mapDrawer.MapMeshDirty(this.sexBuilding.Position, MapMeshFlag.Buildings);
                //beingFuck.actor.pather.StopDead();
                this.pawn.jobs.posture = PawnPosture.Standing;
            });
            //int tickCounter = 0;
            beingFuck.tickAction = (Action) (() =>
            { 
                Pawn actor = beingFuck.actor;
                Job curJob = actor.CurJob;
                JobDriver curDriver = actor.jobs.curDriver;
                //actor.Drawer.renderer.graphics.ClearCache();
                //actor.Drawer.renderer.graphics.apparelGraphics.Clear();
                //actor.GainComfortFromCellIfPossible();
                JoyUtility.JoyTickCheckEnd(this.pawn);
            });
            return beingFuck;
        }
    }*/
}