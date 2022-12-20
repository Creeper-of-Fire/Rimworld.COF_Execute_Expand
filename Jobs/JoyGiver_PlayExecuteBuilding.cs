using RimWorld;
using Verse;
using Verse.AI;

namespace COF_Torture.Jobs
{
    public class JoyGiver_PlayExecuteBuilding_Masochist: JoyGiver_InteractBuilding
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
                return null;
            if (pawn.CanReach((LocalTargetInfo)t, PathEndMode.ClosestTouch,
                    Danger.Some))
                //&&pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving)
            {
                if (pawn.CanReserve((LocalTargetInfo)t))
                    return JobMaker.MakeJob(def.jobDef, (LocalTargetInfo)t);
            }
            else
            {  
                //Log.Message(pawn.ToString()+" try to use"+t.ToString());
                return null;
            }
            return null;
        }
    }
}