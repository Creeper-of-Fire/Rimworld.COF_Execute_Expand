using COF_Torture.Data;
using COF_Torture.Hediffs;
using JetBrains.Annotations;
using Verse;

namespace COF_Torture.Cell
{
    public class CellEffector_AddHediffSiting : CellEffector_AddHediff
    {
        public CellEffector_AddHediffSiting(HediffDef hediff, BodyPartDef part) : base(hediff, part)
        {
        }

        public CellEffector_AddHediffSiting()
        {
        }

        public sealed override void StayEffect(Pawn pawn)
        {
            TryAddHediff(pawn);
        }

        public sealed override Hediff MakeHediff(Pawn pawn)
        {
            var h = (Hediff_WithGiver)base.MakeHediff(pawn);
            if (h != null)
            {
                h.Giver = this.Giver;
                return h;
            }

            return null;
        }
    }

    public class CellEffector_AddHediffWhenPass : CellEffector_AddHediff
    {
        public CellEffector_AddHediffWhenPass()
        {
        }

        public sealed override void PassEffect(Pawn pawn)
        {
            TryAddHediff(pawn);
        }

        public CellEffector_AddHediffWhenPass(HediffDef hediff, BodyPartDef part) : base(hediff, part)
        {
        }
    }

    public abstract class CellEffector_AddHediff : CellEffector
    {
        protected HediffDef hediff;
        protected BodyPartDef part;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref hediff, "hediff");
            Scribe_Defs.Look(ref part, "part");
        }

        public CellEffector_AddHediff()
        {
        }

        public CellEffector_AddHediff(HediffDef hediff, BodyPartDef part)
        {
            this.hediff = hediff;
            this.part = part;
        }

        public override void TryUndoEffect(Pawn pawn)
        {
            var h = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
            if (h != null)
            {
                pawn.health.RemoveHediff(h);
            }
            //ModLog.Message("remove"+hediff+"from"+pawn);
        }

        public virtual void TryAddHediff(Pawn pawn)
        {
            if (pawn.health.hediffSet.GetFirstHediffOfDef(hediff) != null) return;
            var h = MakeHediff(pawn);
            if (h == null) return;
            pawn.health.AddHediff(h, h.Part);
            //ModLog.Message("add"+hediff+"to"+pawn+h.ShouldRemove);
        }

        [CanBeNull]
        public virtual Hediff MakeHediff(Pawn pawn)
        {
            if (hediff == null || part == null)
            {
                IsValid = false;
                return null;
            }

            var partRecord = pawn.health.hediffSet.GetNotMissingParts()
                .FirstOrFallback(p => p.def == part);
            return HediffMaker.MakeHediff(hediff, pawn, partRecord);
        }
    }
}