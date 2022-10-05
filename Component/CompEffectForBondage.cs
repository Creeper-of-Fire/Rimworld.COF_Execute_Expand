using System;
using COF_Torture.Hediffs;
using Verse;

namespace COF_Torture.Component
{
    public class CompProperties_EffectForBondage : CompProperties
    {
        public HediffDef hediff;
        public BodyPartDef part;

        public CompProperties_EffectForBondage() => this.compClass = typeof(CompEffectForBondage);
    }

    public class CompEffectForBondage : ThingComp
    {
        public CompProperties_EffectForBondage Props => (CompProperties_EffectForBondage)this.props;
        public COF_Torture.Things.Building_TortureBed Parent => (COF_Torture.Things.Building_TortureBed)this.parent;
        public Pawn Victim => this.Parent.GetVictim();

        /*public void RemoveEffect()
        {
            var t = this.Victim.health;
            Hediff_WithGiver h = t.hediffSet.GetFirstHediffOfDef(this.Props.hediff) as Hediff_WithGiver;
            if (h != null)
            {
                h.giver = null;
                t.RemoveHediff(h);
            }

            h = t.hediffSet.GetFirstHediffOfDef(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Fixed) as Hediff_WithGiver;
            if (h != null)
            {
                h.giver = null;
                t.RemoveHediff(h);
            }
        }*/

        public void AddEffect()
        {
            HediffDef hediffDef = this.Props.hediff;
            var t = this.Victim.health;
            Verse.Hediff hediff = t.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (hediff == null)
            {
                Hediff_WithGiver hediffAdd;
                if (this.Props.part == null)
                {
                    hediffAdd = (Hediff_WithGiver)HediffMaker.MakeHediff(hediffDef, this.Victim);
                }
                else
                {
                    var bodyPart = t.hediffSet.GetNotMissingParts().FirstOrFallback<BodyPartRecord>(
                        (Func<BodyPartRecord, bool>)(p => p.def == this.Props.part));
                    hediffAdd = (Hediff_WithGiver)HediffMaker.MakeHediff(hediffDef, this.Victim, bodyPart);
                }

                hediffAdd.giver = this.Parent;
                t.AddHediff(hediffAdd);
            }

            Hediff_Protect hediffTF =
                (Hediff_Protect)HediffMaker.MakeHediff(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Fixed, this.Victim);
            hediffTF.giver = this.Parent;
            t.AddHediff(hediffTF);
        }
    }
}