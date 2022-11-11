using COF_Torture.Genes;
using HarmonyLib;
using Verse;
using COF_Torture.Hediffs;

namespace COF_Torture.Patch
{
    [HarmonyPatch]
    public static class NeverPainDownedPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn_HealthTracker), "InPainShock", MethodType.Getter)]
        public static void Postfix(Pawn_HealthTracker __instance, ref bool __result)
        {
            if (ModLister.BiotechInstalled)
            {
                GeneDef def = GeneDefOf.COF_Torture_NeverPainDowned;
                if (__instance.hediffSet.pawn.genes != null && !__instance.hediffSet.pawn.genes.HasGene(def)) return;
            }
            else
            {
                HediffDef def = HediffDefOf.COF_Torture_NeverPainDowned;
                if (!__instance.hediffSet.HasHediff(def)) return;
            }

            __result = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HediffSet), "CalculatePain")]
        public static bool Prefix(HediffSet __instance, ref float __result)
        {
            if (!__instance.pawn.RaceProps.IsFlesh || __instance.pawn.Dead)
                __result = 0.0f;
            if (ModLister.BiotechInstalled)
            {
                GeneDef def = GeneDefOf.COF_Torture_NeverPainDowned;
                if (__instance.pawn.genes != null && !__instance.pawn.genes.HasGene(def)) return true;
            }
            else
            {
                HediffDef def = HediffDefOf.COF_Torture_NeverPainDowned;
                if (!__instance.HasHediff(def)) return true;
            }
            float num = 0.0f;
            for (int index = 0; index < __instance.hediffs.Count; ++index)
                num += __instance.hediffs[index].PainOffset;
            if (__instance.pawn.genes != null)
                num += __instance.pawn.genes.PainOffset;
            __result = num;
            return false;
        }
    }
}