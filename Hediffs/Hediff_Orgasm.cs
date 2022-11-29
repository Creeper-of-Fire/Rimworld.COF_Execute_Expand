using System;
using Verse;

namespace COF_Torture.Hediffs
{
    public class Hediff_Orgasm : HediffWithComps
    {
        public override string LabelInBrackets
        {
            get
            {
                if (this.Severity < 1.5)
                    return base.LabelInBrackets;
                else
                    return Math.Ceiling((double)this.Severity) + "CT_OrgasmTimes".Translate();
            }
        }
    }
}