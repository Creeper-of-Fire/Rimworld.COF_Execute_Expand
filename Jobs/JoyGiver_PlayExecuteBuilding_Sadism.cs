using COF_Torture.Things;
using RimWorld;
using Verse;
using Verse.AI;

namespace COF_Torture.Jobs
{
    public class JoyGiver_PlayExecuteBuilding_Sadism : JoyGiver_InteractBuilding
    {
        protected override Job TryGivePlayJob(Pawn pawn, Thing t)
        {
            /*if (t.GetType() == typeof(Building_TortureBed))
            {
                var a = (Building_TortureBed)t;
                if (a.isUnUsableForOthers())
                {
                    //Log.Message(pawn.ToString()+" try to use"+t.ToString()+"but is reserved.");
                    return (Job)null;
                }
            }*/
            if (!pawn.story.traits.HasTrait(TraitDefOf.Masochist))
                return (Job)null;
            if (pawn.CanReach((LocalTargetInfo)(Thing)t, PathEndMode.ClosestTouch,
                    Danger.Some))
                //&&pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving)
            {
                //if (pawn.CanReserve((LocalTargetInfo)(Thing)t))
                if (t is Building_TortureBed bT && bT.isUnUsableForOthers())
                    return JobMaker.MakeJob(this.def.jobDef, (LocalTargetInfo)bT.GetVictim());
            }
            else
            {
                //Log.Message(pawn.ToString()+" try to use"+t.ToString());
                return (Job)null;
            }

            return (Job)null;
        }
        protected override bool CanInteractWith(Pawn pawn, Thing t, bool inBed)
        {
            if (t.IsForbidden(pawn) || !t.IsSociallyProper(pawn) || !t.IsPoliticallyProper(pawn))
                return false;
            CompPowerTrader comp = t.TryGetComp<CompPowerTrader>();
            return (comp == null || comp.PowerOn) && (!this.def.unroofedOnly || !t.Position.Roofed(t.Map));
        }
    }
}