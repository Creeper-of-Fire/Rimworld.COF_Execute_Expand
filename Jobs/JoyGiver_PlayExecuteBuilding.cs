using COF_Torture.Things;
using RimWorld;
using Verse;
using Verse.AI;

namespace COF_Torture.Jobs
{
    public class JoyGiver_PlayExecuteBuilding: JoyGiver_InteractBuilding
    {
    
        protected override Job TryGivePlayJob(Pawn pawn, Thing t)
        {
            if (t.GetType() == typeof(Building_TortureBed))
            {
                var a = (Building_TortureBed)t;
                if (a.isUsing)
                {
                    Log.Message(pawn.ToString()+" try to use"+t.ToString()+"but is reserved.");
                    return (Job)null;
                }
            }
            if (pawn.CanReserveAndReach((LocalTargetInfo)(Thing)t, PathEndMode.OnCell,
                    Danger.Some) &&
                pawn.story.traits.HasTrait(TraitDefOf.Masochist))
                //&&pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving)
            {
                return JobMaker.MakeJob(this.def.jobDef, (LocalTargetInfo)t);
            }
            else
            {  
                //Log.Message(pawn.ToString()+" try to use"+t.ToString());
                return (Job)null;
            }
        }
    }
}