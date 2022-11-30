using System.Collections.Generic;
using System.Linq;
using COF_Torture.Body;
using COF_Torture.Utility;
using RimWorld;
using Verse;
using HediffDefOf = COF_Torture.Utility.DefOf.HediffDefOf;

namespace COF_Torture.Data
{
    /// <summary>
    /// Ed86, your code is nice and clear. Now it is mine!
    /// </summary>
    public class PawnData : IExposable
    {
        private Pawn Pawn;
        public bool IsFixed;
        public bool IsMasochism;
        public Thing Fixer;
        private VirtualPartData _virtualParts;

        public VirtualPartData VirtualParts
        {
            get
            {
                if (_virtualParts == null)
                    if (Pawn != null)
                        _virtualParts = new VirtualPartData(Pawn);
                    else
                        ModLog.Error("Pawn为空，无法新建VirtualParts");
                return _virtualParts;
            }
        }


        public PawnData()
        {
        }

        public PawnData(Pawn pawn)
        {
            Pawn = pawn;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref Pawn, "Pawn");
            Scribe_Values.Look(ref IsFixed, "IsFixed", IsFixedDefault(), forceSave: true);
            Scribe_Values.Look(ref IsMasochism, "IsMasochism", IsMasochistDefault(), forceSave: true);
            Scribe_References.Look(ref Fixer, "Fixer");
            Scribe_Deep.Look(ref _virtualParts, "VirtualParts");
            if (Fixer == null)
                Fixer = FixerDefault();
        }

        private bool IsMasochistDefault() =>
            Pawn?.story?.traits != null && Pawn.story.traits.HasTrait(TraitDefOf.Masochist);

        private bool IsFixedDefault() =>
            Pawn?.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.COF_Torture_Fixed) != null;

        private Thing FixerDefault()
        {
            var h = Pawn?.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.COF_Torture_Fixed);
            if (h is IWithGiver iH)
                return iH.Giver;
            return null;
        }

        public bool IsValid => Pawn != null;

        /*public string GetUniqueLoadID()
        {
            return "PawnData" + this.Pawn.ThingID;
        }*/
    }
}