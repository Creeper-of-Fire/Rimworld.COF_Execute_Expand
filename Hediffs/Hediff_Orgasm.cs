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
                if (Severity < 1.5)
                    return base.LabelInBrackets;
                return Math.Ceiling(Severity) + "CT_OrgasmTimes".Translate();
            }
        }
    }
}