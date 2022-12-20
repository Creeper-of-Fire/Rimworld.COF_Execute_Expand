using COF_Torture.Data;
using COF_Torture.Utility;
using HarmonyLib;
using Verse;

namespace COF_Torture.Patch
{
    [HarmonyPatch]
    public static class PawnTickPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn), "TickRare")]
        public static void Postfix(Pawn __instance)
        {
            //if (DebugSettings.noAnimals && __instance.Spawned && __instance.RaceProps.Animal) return;
            if (__instance.Suspended) return;
            var a = __instance.GetCellEffectors();
            if (a != null)
            {
                a.DoTick(__instance);
                //ModLog.Message(""+a.ToString()+a.Count()+a.Empty());
            }
        }
    }
}