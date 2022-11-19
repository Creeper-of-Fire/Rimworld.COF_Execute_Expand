using System.Collections.Generic;
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
            if (!ModSettingMain.Instance.Setting.controlMenuOn) yield break;
            foreach (var h in __instance.health.hediffSet.hediffs)
            {
                if (h is IWithGiver)
                {
                    Command_Action commandAction = new Command_Action();
                    commandAction.defaultLabel = "CT_TortureThingManager".Translate();
                    commandAction.icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport");
                    commandAction.defaultDesc = "CT_TortureThingManagerDesc".Translate();
                    commandAction.action = delegate
                    {
                        bool flag = false;
                        Dialog_TortureThingManager Dialog = new Dialog_TortureThingManager(__instance);
                        foreach (var window in Find.WindowStack.Windows)
                        {
                            if (window is Dialog_TortureThingManager dialogTortureThingManager)
                            {
                                //if (dialogTortureThingManager.pawn == __instance)
                                Dialog = dialogTortureThingManager;
                                flag = true;
                            }
                        }

                        if (flag)
                        {
                            Find.WindowStack.TryRemove(Dialog);
                        }
                        else
                        {
                            Find.WindowStack.Add(Dialog);
                        }
                    };
                    yield return commandAction;
                    break;
                }
            }
        }
    }
}