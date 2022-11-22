using System.Collections.Generic;
using COF_Torture.Component;
using COF_Torture.Hediffs;
using COF_Torture.ModSetting;
using COF_Torture.Things;
using RimWorld;
using UnityEngine;
using Verse;

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