using System.Collections.Generic;
using System.Linq;
using COF_Torture.Dialog;
using COF_Torture.Hediffs;
using COF_Torture.ModSetting;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace COF_Torture.Patch
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public class PawnGizmoPatch
    {
        [HarmonyPostfix]
        private static IEnumerable<Gizmo> AddTortureGizmo(
            IEnumerable<Gizmo> __result,
            Pawn __instance)
        {
            foreach (Gizmo gizmo in __result)
                yield return gizmo;
            var ___hediff = __instance.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.COF_Torture_Fixed);
            foreach (var gizmo in __instance.Gizmo_AbuseMenu((IWithGiver)___hediff))
                yield return gizmo;

            if (!ModSettingMain.Instance.Setting.controlMenuOn) yield break;
            if (__instance.health.hediffSet.hediffs.OfType<IWithGiver>().Any())
            {
                foreach (var gizmo in __instance.Gizmo_TortureThingManager())
                    yield return gizmo;
            }
        }
    }
}