using COF_Torture.Utility;
using RimWorld;
using Verse;
using HediffDefOf = COF_Torture.Hediffs.HediffDefOf;

namespace COF_Torture.Data
{
    /// <summary>
    /// Ed86, your code is nice and clear. Now it is mine!
    /// </summary>
    public class PawnData : IExposable
    {
        public Pawn Pawn;
        public bool IsFixed;
        public bool IsMasochism;
        public Thing Fixer;

        public PawnData()
        {
        }

        public PawnData(Pawn pawn)
        {
            this.Pawn = pawn;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref this.Pawn, "Pawn");
            Scribe_Values.Look(ref this.IsFixed, "IsFixed", IsFixedDefault(), forceSave: true);
            Scribe_Values.Look(ref this.IsMasochism, "IsMasochism", IsMasochistDefault(), forceSave: true);
            Scribe_References.Look(ref Fixer, "Fixer");
            if (this.Fixer == null)
                Fixer = FixerDefault();
        }
        private bool IsMasochistDefault() => Pawn?.story?.traits != null && Pawn.story.traits.HasTrait(TraitDefOf.Masochist);
        private bool IsFixedDefault() =>
            this.Pawn?.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.COF_Torture_Fixed) != null;

        private Thing FixerDefault()
        {
            var h = this.Pawn?.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.COF_Torture_Fixed);
            if (h is IWithGiver iH)
                return iH.Giver;
            return null;
        }

        public bool IsValid => this.Pawn != null;

        /*public string GetUniqueLoadID()
        {
            return "PawnData" + this.Pawn.ThingID;
        }*/
    }
}