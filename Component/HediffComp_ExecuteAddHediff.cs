using System.Collections.Generic;
using COF_Torture.Hediffs;
using COF_Torture.Patch;
using Verse;

namespace COF_Torture.Component
{
    public class HediffCompProperties_ExecuteAddHediff : HediffCompProperties
    {
        public int ticksToAdd = 100;
        public FloatRange severityToAdd;
        public int addHediffNumMax = 20;
        public HediffDef addHediff;
        public List<BodyPartDef> addBodyParts;

        public HediffCompProperties_ExecuteAddHediff() => this.compClass = typeof(HediffComp_ExecuteAddHediff);
    }

    public class HediffComp_ExecuteAddHediff : HediffComp
    {
        public HediffCompProperties_ExecuteAddHediff Props => (HediffCompProperties_ExecuteAddHediff)this.props;
        public Hediff_WithGiver Parent => (Hediff_WithGiver)this.parent;
        public int ticksToAdd;
        public Thing giver;

        public static DamageInfo dInfo()
        {
            if (SettingPatch.RimJobWorldIsActive)
            {
                var execute = Damages.DamageDefOf.Execute;
                return new DamageInfo(execute, 1);
            }

            return new DamageInfo();
        }

        public IEnumerable<BodyPartRecord> ListOfPart()
        {
            var partsHave = Pawn.health.hediffSet.GetNotMissingParts();
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

        public void addHediff()
        {
            if (giver == null)
            {
                giver = this.Parent.giver;
            }
            var part = ListOfPart().RandomElement();
            var hDef = this.Props.addHediff;
            //Log.Message(part + "");
            Hediff_ExecuteInjury h = (Hediff_ExecuteInjury)HediffMaker.MakeHediff(hDef, Pawn, part);
            h.Severity = this.Props.severityToAdd.RandomInRange;
            if (Pawn.health.hediffSet.GetHediffCount(hDef) < this.Props.addHediffNumMax)
            {
                if (!Pawn.health.WouldLosePartAfterAddingHediff(hDef, part, h.Severity))
                {
                    if (part == null || (double)part.coverageAbs > 0.0)
                    {
                        //Log.Message(h + "");
                        if (!Pawn.health.WouldDieAfterAddingHediff(hDef, part, h.Severity))
                        {
                            h.giver = giver;
                            Pawn.health.AddHediff(h, part, dInfo());
                        }
                        return;
                    }
                }

                this.addHediff();
            }
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


        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            for (int i = 0; i < this.Props.addHediffNumMax; i++)
            {
                addHediff();
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            ticksToAdd++;
            if (ticksToAdd >= this.Props.ticksToAdd)
            {
                ticksToAdd = 0;
                addHediff();
                //Log.Message("Add Hediff");
            }
        }
    }
}