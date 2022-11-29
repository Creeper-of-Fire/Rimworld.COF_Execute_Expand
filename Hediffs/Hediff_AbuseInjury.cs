using System.Collections.Generic;
using System.Text;
using COF_Torture.Component;
using RimWorld;
using UnityEngine;
using Verse;

namespace COF_Torture.Hediffs
{
    public class Hediff_AbuseInjury : Hediff_Injury
    {
        public List<BodyPartGroupDef> ableBodyPartGroup
        {
            get
            {
                if (this.def is MaltreatDef hediffWithBodyPartGroups)
                    return hediffWithBodyPartGroups.maltreat.ableBodyPartGroupDefs;
                return new List<BodyPartGroupDef>();
            }
        }

        public override string LabelBase
        {
            get
            {
                HediffComp_BecomePermanent comp = this.TryGetComp<HediffComp_BecomePermanent>();
                if (comp != null && comp.IsPermanent)
                {
                    if (!comp.Props.permanentLabel.NullOrEmpty())
                        return comp.Props.permanentLabel;
                }

                return this.def.label;
            }
        }

        public override string LabelInBrackets
        {
            get
            {
                HediffComp_BecomePermanent comp = this.TryGetComp<HediffComp_BecomePermanent>();
                if (comp != null && comp.IsPermanent)
                {
                    return "";
                }
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(base.LabelInBrackets);
                if (this.sourceHediffDef != null)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(this.sourceHediffDef.label);
                }
                else if (this.source != null)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(this.source.label);
                    if (this.sourceBodyPartGroup != null)
                    {
                        stringBuilder.Append(" ");
                        stringBuilder.Append(this.sourceBodyPartGroup.LabelShort);
                    }
                }

                return stringBuilder.ToString();
            }
        }

        public override Color LabelColor => this.def.defaultLabelColor;

        public override void PostAdd(DamageInfo? dinfo)
        {
            void HediffBase()
            {
                if (!this.def.disablesNeeds.NullOrEmpty())
                    this.pawn.needs.AddOrRemoveNeedsAsAppropriate();
                if (this.def.removeWithTags.NullOrEmpty())
                    return;
                for (int index1 = this.pawn.health.hediffSet.hediffs.Count - 1; index1 >= 0; --index1)
                {
                    Hediff hediff = this.pawn.health.hediffSet.hediffs[index1];
                    if (hediff != this && !hediff.def.tags.NullOrEmpty())
                    {
                        foreach (var t in this.def.removeWithTags)
                        {
                            if (hediff.def.tags.Contains(t))
                            {
                                this.pawn.health.RemoveHediff(hediff);
                                break;
                            }
                        }
                    }
                }
            }

            HediffBase();
            if (this.comps == null)
                return;
            for (int index = 0; index < this.comps.Count; ++index)
                this.comps[index].CompPostPostAdd(dinfo);
        }

        //public override bool ShouldRemove => this.IsPermanent() && (double)this.Severity <= 0.0;

        public override void Heal(float amount)
        {
            if (this.IsPermanent())
                return;
            base.Heal(amount);
        }
    }
}