using System.Collections.Generic;
using COF_Torture.Component;
using COF_Torture.Hediffs;
using COF_Torture.ModSetting;
using COF_Torture.Things;
using RimWorld;
using UnityEngine;
using Verse;
using HediffDefOf = COF_Torture.Hediffs.HediffDefOf;

namespace COF_Torture.Dialog
{
    public static class Gizmo_List
    {
        /// <summary>
        /// 处刑建筑是否启用安全模式
        /// </summary>
        /// <param name="ButtonOwner">应当是一个处刑建筑</param>
        public static IEnumerable<Command> Gizmo_SafeMode(this Building_TortureBed ButtonOwner)
        {
            var SafeMode = new Command_Toggle();
            SafeMode.defaultDesc = "CT_isSafeDesc".Translate();
            if (ButtonOwner.isSafe)
            {
                SafeMode.defaultLabel = "CT_isSafe".Translate() + "Enabled".Translate();
            }
            else
            {
                SafeMode.defaultLabel = "CT_isSafe".Translate() + "Disabled".Translate();
            }

            SafeMode.hotKey = KeyBindingDefOf.Misc4;
            SafeMode.icon = GizmoIcon.texSkull;
            SafeMode.isActive = () => ButtonOwner.isSafe;
            SafeMode.toggleAction = () => ButtonOwner.isSafe = !ButtonOwner.isSafe;
            yield return SafeMode;
        }

        public static IEnumerable<Command> Gizmo_AbuseMenu(this Pawn pawn, IWithGiver hediff)
        {
            if (hediff != null)
            {
                foreach (var command in Gizmo_AbuseMenu(hediff.GiverAsInterface))
                {
                    yield return command;
                }
            }
        }

        public static IEnumerable<Command> Gizmo_AbuseMenu(this ITortureThing thing)
        {
            Command_Action command = new Command_Action();
            command.defaultLabel = "CT_AbuseMenu".Translate();
            command.icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport");
            command.defaultDesc = "CT_AbuseMenuDesc".Translate();
            command.action = delegate
            {
                var Dialog = new Dialog_AbuseMenu(thing.victim, thing);
                Find.WindowStack.Add(Dialog);
            };
            yield return command;
        }

        public static IEnumerable<Command> Gizmo_TortureThingManager(this Pawn pawn)
        {
            Command_Action commandAction = new Command_Action();
            commandAction.defaultLabel = "CT_TortureThingManager".Translate();
            commandAction.icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport");
            commandAction.defaultDesc = "CT_TortureThingManagerDesc".Translate();
            commandAction.action = delegate
            {
                bool flag = false;
                Dialog_TortureThingManager Dialog = new Dialog_TortureThingManager(pawn);
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
        }

        public static IEnumerable<Command> Gizmo_TortureThingManager(this ITortureThing thing)
        {
            return Gizmo_TortureThingManager(thing.victim);
        }

        /// <summary>
        /// 处刑建筑释放被束缚的处刑对象
        /// </summary>
        /// <param name="ButtonOwner">应当是一个处刑建筑</param>
        public static IEnumerable<Command> Gizmo_ReleaseBondageBed(this Building_TortureBed ButtonOwner)
        {
            var com = new Command_Action();
            com.defaultLabel = "CT_Release".Translate();
            com.defaultDesc = "CT_Release_BondageBed".Translate(ButtonOwner.victim);
            com.hotKey = KeyBindingDefOf.Misc5;
            com.icon = GizmoIcon.texPodEject;
            com.action = ButtonOwner.ReleaseVictim;
            if (ModSettingMain.Instance.Setting.isNoWayBack)
                com.Disable("CT_No_Way_Back".Translate());
            yield return com;
        }

        /// <summary>
        /// 束缚Hediff释放束缚对象
        /// </summary>
        /// <param name="hediff">应当是束缚hediff</param>
        public static IEnumerable<Command> Gizmo_ReleaseBondageBed(this Hediff_Fixed hediff)
        {
            var com = new Command_Action();
            com.defaultLabel = "CT_Release".Translate();
            com.defaultDesc = "CT_Release_BondageBed".Translate(hediff.GiverAsInterface.victim);
            com.hotKey = KeyBindingDefOf.Misc5;
            com.icon = GizmoIcon.texPodEject;
            com.action = hediff.GiverAsInterface.ReleaseVictim;
            if (ModSettingMain.Instance.Setting.isNoWayBack)
                com.Disable("CT_No_Way_Back".Translate());
            yield return com;
        }

        /// <summary>
        /// 处刑的开始或者结束
        /// </summary>
        /// <param name="hediffComp">管理处刑过程的Comp</param>
        public static IEnumerable<Command> Gizmo_StartAndStopExecute(this HediffComp_ExecuteIndicator hediffComp)
        {
            var com = new Command_Action();
            if (!hediffComp.isInProgress)
            {
                com.defaultLabel = "CT_startExecute".Translate();
                com.defaultDesc = "CT_startExecute".Translate();
                com.hotKey = KeyBindingDefOf.Misc5;
                com.icon = GizmoIcon.texSkull;
                com.action = hediffComp.StartProgress;
            }
            else
            {
                com.defaultLabel = "CT_stopExecute".Translate();
                com.defaultDesc = "CT_stopExecute".Translate();
                com.hotKey = KeyBindingDefOf.Misc5;
                com.icon = GizmoIcon.texSkull;
                com.action = hediffComp.StopProgress;
            }

            yield return com;
        }

        /// <summary>
        /// 开始或者结束处刑
        /// </summary>
        /// <param name="tortureThing">处刑建筑物</param>
        public static IEnumerable<Command> Gizmo_StartAndStopExecute(this ITortureThing tortureThing)
        {
            var com = new Command_Action();
            if (!tortureThing.inExecuteProgress)
            {
                com.defaultLabel = "CT_startExecute".Translate();
                com.defaultDesc = "CT_startExecute".Translate();
                com.hotKey = KeyBindingDefOf.Misc5;
                com.icon = GizmoIcon.texSkull;
                com.action = delegate
                {
                    foreach (var iWithGiver in tortureThing.hasGiven)
                    {
                        if (iWithGiver is Hediff iHediff)
                        {
                            var comp = iHediff.TryGetComp<HediffComp_ExecuteIndicator>();
                            comp.StartProgress();
                        }
                    }
                };
            }
            else
            {
                com.defaultLabel = "CT_stopExecute".Translate();
                com.defaultDesc = "CT_stopExecute".Translate();
                com.hotKey = KeyBindingDefOf.Misc5;
                com.icon = GizmoIcon.texSkull;
                com.action = delegate
                {
                    foreach (var iWithGiver in tortureThing.hasGiven)
                    {
                        if (iWithGiver is Hediff iHediff)
                        {
                            var comp = iHediff.TryGetComp<HediffComp_ExecuteIndicator>();
                            comp.StopProgress();
                        }
                    }
                };
            }

            yield return com;
        }

        /// <summary>
        /// 手动控制严重度
        /// </summary>
        /// <param name="hediffComp">手动控制严重度的Comp</param>
        public static IEnumerable<Command> Gizmo_StageUpAndDown(this HediffComp_SwitchAbleSeverity hediffComp)
        {
            var stageUp = new Command_Action();
            stageUp.defaultLabel = "CT_stageUp".Translate();
            //stageUp.defaultDesc = "CT_stageUp_desc".Translate();
            stageUp.icon = ContentFinder<Texture2D>.Get("COF_Torture/UI/SwitchStage");
            stageUp.action = hediffComp.upStage;
            if (hediffComp.stageLimit >= hediffComp.stageMax)
                stageUp.Disable("CannotReach".Translate());
            var stageDown = new Command_Action();
            stageDown.defaultLabel = "CT_stageDown".Translate();
            //stageDown.defaultDesc = "CT_stageDown_desc".Translate();
            stageDown.icon = ContentFinder<Texture2D>.Get("COF_Torture/UI/SwitchStage");
            stageDown.action = hediffComp.downStage;
            if (hediffComp.stageLimit <= 0)
                stageDown.Disable("CannotReach".Translate());
            yield return stageUp;
            yield return stageDown;
        }
    }
}