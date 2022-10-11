using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace COF_Torture.Patch
{
    [StaticConstructorOnStartup]
    public static class SettingPatch
    {
        public static bool DubsBadHygieneIsActive;
        public static bool DubsBadHygieneThirstIsActive;
        public static NeedDef DBHThirst;
        public static NeedDef Hygiene;
        public static NeedDef Bladder;
        public static NeedDef Sex;
        public static bool RimJobWorldIsActive;
    }

    [StaticConstructorOnStartup]
    public static class MainPatch
    {
        static MainPatch()
        {
            ModSettingPatch();
            var harmony = new Harmony("com.github.Creeper-of-Fire");
            //Harmony.DEBUG = true;
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("[COF_Torture]修改加载中!");
        }

        private static void ModSettingPatch()
        {
            SettingPatch.Hygiene = DefDatabase<NeedDef>.GetNamedSilentFail("Hygiene"); //DUBS卫生
            SettingPatch.Bladder = DefDatabase<NeedDef>.GetNamedSilentFail("Bladder"); //DUBS膀胱
            if (SettingPatch.Hygiene == null && SettingPatch.Bladder == null)
            {
                SettingPatch.DubsBadHygieneIsActive = false;
            }
            else
            {
                SettingPatch.DubsBadHygieneIsActive = true;
                Log.Message("[COF_TORTURE]Dubs Bad Hygiene is detected.");
            }

            SettingPatch.DBHThirst = DefDatabase<NeedDef>.GetNamedSilentFail("DBHThirst"); //DUBS口渴
            if (SettingPatch.DBHThirst == null)
            {
                SettingPatch.DubsBadHygieneThirstIsActive = false;
            }
            else
            {
                SettingPatch.DubsBadHygieneThirstIsActive = true;
                Log.Message("[COF_TORTURE]Dubs Bad Hygiene Thirst is detected.");
            }

            SettingPatch.Sex = DefDatabase<NeedDef>.GetNamedSilentFail("Sex");
            if (SettingPatch.Sex == null)
            {
                SettingPatch.RimJobWorldIsActive = false;
            }
            else
            {
                SettingPatch.RimJobWorldIsActive = true;
                Log.Message("[COF_TORTURE]Rim Job World is detected.");
            }
        }
    }
}