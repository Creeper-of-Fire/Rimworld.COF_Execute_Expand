using COF_Torture.Hediffs;
using HarmonyLib;
using Verse;

namespace COF_Torture.Patch
{
    [HarmonyPatch]
    public static class LifeContinue
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDead")]
        public static bool Prefix(Pawn_HealthTracker __instance, ref bool __result)
        {
            if (__instance.Dead)
                return true;
            HediffDef def = HediffDefOf.COF_Torture_Fixed;
            if (__instance.hediffSet.HasHediff(def))// && ModSettingMain.Instance.Setting.isSafe)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}