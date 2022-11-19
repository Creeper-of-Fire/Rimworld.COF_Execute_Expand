using System.Collections.Generic;
using System.Linq;
using COF_Torture.Hediffs;
using COF_Torture.ModSetting;
using COF_Torture.Patch;
using COF_Torture.Things;
using RimWorld;
using Verse;

namespace COF_Torture.Component
{
    public class HediffCompProperties_ExecuteAddHediff : HediffCompProperties
    {
        public int ticksToAdd = 100;
        public FloatRange severityToAdd;
        public int addHediffNumMax = 20;
        public int addHediffNumInt = 20;
        public HediffDef addHediff;
        public List<BodyPartDef> addBodyParts;

        public HediffCompProperties_ExecuteAddHediff() => this.compClass = typeof(HediffComp_ExecuteAddHediff);
    }

    public class HediffComp_ExecuteAddHediff : HediffComp, IExecuteEffector
    {
        public HediffCompProperties_ExecuteAddHediff Props => (HediffCompProperties_ExecuteAddHediff)this.props;
        public Hediff_WithGiver Parent => (Hediff_WithGiver)this.parent;
        public int ticksToAdd;
        public Thing giver;

        public bool isInProgress;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref isInProgress, "isInProgress", false);
        }

        public static DamageInfo dInfo()
        {
            if (SettingPatch.RimJobWorldIsActive)
            {
                var execute = Damages.DamageDefOf.Execute_Licentious;
                return new DamageInfo(execute, 1);
            }
            else
            {
                var execute = Damages.DamageDefOf.Execute;
                return new DamageInfo(execute, 1);
            }
        }

        public virtual IEnumerable<BodyPartRecord> ListOfPart()
        {
            var partsHave = Pawn.health.hediffSet.GetNotMissingParts();
            if (partsHave == null)
            {
                yield break;
            }

            var partsAdd = this.Props.addBodyParts;
            //var h = (Hediff_Injury)HediffMaker.MakeHediff(Props.addHediff, Pawn);
            if (partsAdd == null)
            {
                foreach (var p in partsHave)
                {
                    yield return p;
                }

                yield break;
            }

            foreach (var p in partsHave)
            {
                if (partsAdd.Contains(p.def))
                    yield return p;
            }
        }

        public virtual void addHediff(int depth = 0)
        {
            depth++;
            if (depth >= 10)
                return;
            if (giver == null)
            {
                giver = this.Parent.Giver;
            }

            BodyPartRecord part = ListOfPart().RandomElement();
            if (part == null)
                return;
            HediffDef hDef = this.Props.addHediff;
            //Log.Message(part + "");
            Hediff_ExecuteInjury h = (Hediff_ExecuteInjury)HediffMaker.MakeHediff(hDef, Pawn, part);
            h.Severity = this.Props.severityToAdd.RandomInRange;
            if (Pawn.health.hediffSet.GetHediffCount(hDef) < this.Props.addHediffNumMax)
            {
                if (isAddAble(part, h))
                {
                    h.Giver = giver;
                    Pawn.health.AddHediff(h, part, dInfo());
                }

                this.addHediff(depth);
            }
        }

        private bool isAddAble(BodyPartRecord part, Hediff_Injury h)
        {
            if (part == null)
                return false;
            if (!((double)part.coverageAbs > 0.0))
            {
                return false;
            }

            if (this.Parent.Giver is Building_TortureBed bT && bT.isSafe)
            {
                if (Pawn.health.hediffSet.GetPartHealth(part) < (double)h.Severity)
                {
                    return false;
                }
                /*if (part.def == BodyPartDefOf.Torso && Pawn.health.hediffSet.GetPartHealth(part) <= h.Severity)
                {
                    return false;
                }*/
                /*if (Pawn.health.WouldDieAfterAddingHediff(hDef, part, h.Severity))
                {
                    return false;
                }*/
                /*if (Pawn.health.WouldLosePartAfterAddingHediff(hDef, part, h.Severity))
                {
                    return false;
                }*/
            }

            return true;
        }

        /* public Hediff_Injury MakeHediffHarmless(Hediff_Injury h,BodyPartRecord part, Pawn pawn)
         {
             if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) <= 0.05f)
             {
                 var cap = new PawnCapacityModifier();
                 cap.offset = 0.01f;
                 h.CapMods.Add(cap);
             }
             //if (pawn.health.WouldDieAfterAddingHediff(h, part, h.Severity))
             {
                 //MakeHediffHarmless(h, part, pawn);
             }
             return h;
         }*/
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (!isInProgress) return;
            ticksToAdd++;
            if (ticksToAdd >= this.Props.ticksToAdd)
            {
                ticksToAdd = 0;
                addHediff();
            }
        }

        public void startExecuteProcess()
        {
            for (int i = 0; i < this.Props.addHediffNumInt; i++)
            {
                addHediff();
            }

            this.isInProgress = true;
        }

        public void stopExecuteProcess()
        {
            this.isInProgress = false;
        }
    }
}