using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
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
            foreach (var h in __instance.health.hediffSet.hediffs)
            {
                if (h is IWithGiver)
                {
                    Command_Action commandAction = new Command_Action();
                    commandAction.defaultLabel = "CT_TortureThingManager".Translate();
                    commandAction.icon = (Texture) ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport");
                    commandAction.defaultDesc = "CT_TortureThingManagerDesc".Translate();
                    commandAction.action = delegate
                    {
                        Find.WindowStack.Add(new Dialog_TortureThingManager(__instance));
                    };
                    yield return (Gizmo) commandAction;
                    break;
                }
            }
        }
    }
}