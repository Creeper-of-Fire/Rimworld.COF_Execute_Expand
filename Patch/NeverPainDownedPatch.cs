using HarmonyLib;
using Verse;
using COF_Torture.Hediffs;

namespace COF_Torture.Patch
{
    [HarmonyPatch]
    public static class NeverPainDownedPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn_HealthTracker),"InPainShock", MethodType.Getter)]
        public static void Postfix(Pawn_HealthTracker __instance, ref bool __result)
        {
            HediffDef def = HediffDefOf.COF_Torture_NeverPainDowned;
            if (__instance.hediffSet.HasHediff(def))
            {
                //Log.Message("屹立不倒!");
                __result = false;
                //return false;
            }
            //return true;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HediffSet),"CalculatePain")]
        public static bool Prefix(HediffSet __instance, ref float __result)
        {
            HediffDef def = HediffDefOf.COF_Torture_NeverPainDowned;
            if (__instance.HasHediff(def))
            {
                //Log.Message("屹立不倒!");
                if (!__instance.pawn.RaceProps.IsFlesh || __instance.pawn.Dead)
                    __result = 0.0f;
                float num = 0.0f;
                for (int index = 0; index < __instance.hediffs.Count; ++index)
                    num += __instance.hediffs[index].PainOffset;
                for (int index = 0; index < __instance.hediffs.Count; ++index)
                    num *= __instance.hediffs[index].PainFactor;
                __result = num;
                return false;
            }
            return true;
        }
    }
}