using System;
using COF_Torture.Patch;
using RimWorld;
using Verse;

namespace COF_Torture
{
    public static class TortureUtility
    {
        public const float satisfySexNeedWhenOrgasm = 0.2f;

        public static void Orgasm(Pawn pawn, int OrgasmTimes = 1)
        {
            if (OrgasmTimes < 1)
                OrgasmTimes = 1;
            Hediff HediffOrgasm;
            bool hasOrgasmBefore;
            HediffComp_Disappears comps1 = null;
            if (pawn.health.hediffSet.HasHediff(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Orgasm))
            {
                HediffOrgasm =
                    pawn.health.hediffSet.GetFirstHediffOfDef(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Orgasm);
                hasOrgasmBefore = true;
            }
            else
            {
                HediffOrgasm = HediffMaker.MakeHediff(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Orgasm, pawn);
                hasOrgasmBefore = false;
            }

            for (int i = 0; i < OrgasmTimes; i++)
            {
                if (SettingPatch.RimJobWorldIsActive)
                {
                    Need need = pawn.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == SettingPatch.SexNeed));
                    need.CurLevel += TortureUtility.satisfySexNeedWhenOrgasm; //因为高潮获得了性满足
                }

                if (hasOrgasmBefore)
                {
                    HediffOrgasm.Severity += 1;
                    if (comps1 == null)
                        comps1 = HediffOrgasm.TryGetComp<HediffComp_Disappears>();
                    comps1.ticksToDisappear = comps1.Props.disappearsAfterTicks.RandomInRange;
                }
                //补充高潮状态，重置消失时间
                else
                {
                    pawn.health.AddHediff(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Orgasm);
                    hasOrgasmBefore = true;
                }
            }
        }
    }
}