using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace COF_Torture.Jobs
{
    public class JobDriver_PlayExecuteBuilding_Masochist : JobDriver
    {
        //private const TargetIndex BuildingToPlay = TargetIndex.A;
        
        private Building BuildingToPlay => (Building) this.TargetA;

        //private Toil beingFuck;
        
        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            this.pawn.Reserve(this.job.targetA,
                this.job,
                this.job.def.joyMaxParticipants,
                0);
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return new Toil().FailOnDestroyedNullOrForbidden<Toil>(TargetIndex.A);
            if (this.pawn.story.traits.HasTrait(TraitDefOf.Masochist) && this.pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
            {
                if (this.pawn.Position != this.TargetA.Cell)
                    yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
                yield return BeingFuck();
            }
        }

        private Toil BeingFuck()
        {
            Toil beingFuck = new Toil();
            beingFuck.defaultDuration = 3000;
            beingFuck.defaultCompleteMode = ToilCompleteMode.Delay;
            beingFuck.FailOnDestroyedNullOrForbidden<Toil>(TargetIndex.A);
            beingFuck.initAction = (Action) (() =>
            {
                //this.Map.mapDrawer.MapMeshDirty(this.sexBuilding.Position, MapMeshFlag.Buildings);
                //beingFuck.actor.pather.StopDead();
                this.pawn.jobs.posture = PawnPosture.LayingOnGroundFaceUp;
            });
            //int tickCounter = 0;
            beingFuck.tickAction = (Action) (() =>
            { 
                Pawn actor = beingFuck.actor;
                Job curJob = actor.CurJob;
                JobDriver curDriver = actor.jobs.curDriver;
                //actor.Drawer.renderer.graphics.ClearCache();
                //actor.Drawer.renderer.graphics.apparelGraphics.Clear();
                actor.GainComfortFromCellIfPossible();
                JoyUtility.JoyTickCheckEnd(this.pawn, joySource: ((Building) this.TargetA));
            });
            return beingFuck;
        }
    }
}