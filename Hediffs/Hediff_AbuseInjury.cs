using System.Collections.Generic;
using System.Text;
using COF_Torture.Body;
using COF_Torture.Component;
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
                if (def is MaltreatDef hediffWithBodyPartGroups)
                    return hediffWithBodyPartGroups.maltreat.ableBodyPartGroupDefs;
                return new List<BodyPartGroupDef>();
            }
        }

        public override string Description
        {
            get
            {
                var desc = new StringBuilder();
                desc.Append(base.Description);
                var part = this.TryGetVirtualPart();
                if (part != null)
                    desc.Append("\n\n"+"CT_AtVirtualPart".Translate()+part.Label);
                return desc.ToString();
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

                return def.label;
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
                if (sourceHediffDef != null)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(sourceHediffDef.label);
                }
                else if (source != null)
                {
                    if (stringBuilder.Length != 0)
                        stringBuilder.Append(", ");
                    stringBuilder.Append(source.label);
                    if (sourceBodyPartGroup != null)
                    {
                        stringBuilder.Append(" ");
                        stringBuilder.Append(sourceBodyPartGroup.LabelShort);
                    }
                }

                return stringBuilder.ToString();
            }
        }

        public override Color LabelColor => def.defaultLabelColor;

        public override void PostAdd(DamageInfo? dinfo)
        {
            void HediffBase()
            {
                if (!def.disablesNeeds.NullOrEmpty())
                    pawn.needs.AddOrRemoveNeedsAsAppropriate();
                if (def.removeWithTags.NullOrEmpty())
                    return;
                for (int index1 = pawn.health.hediffSet.hediffs.Count - 1; index1 >= 0; --index1)
                {
                    Hediff hediff = pawn.health.hediffSet.hediffs[index1];
                    if (hediff != this && !hediff.def.tags.NullOrEmpty())
                    {
                        foreach (var t in def.removeWithTags)
                        {
                            if (hediff.def.tags.Contains(t))
                            {
                                pawn.health.RemoveHediff(hediff);
                                break;
                            }
                        }
                    }
                }
            }

            HediffBase();
            if (comps == null)
                return;
            for (int index = 0; index < comps.Count; ++index)
                comps[index].CompPostPostAdd(dinfo);
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