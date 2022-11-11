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
        public static NeedDef DBHThirstNeed;
        public static NeedDef HygieneNeed;
        public static NeedDef BladderNeed;
        public static NeedDef SexNeed;
        public static bool RimJobWorldIsActive;
        public static SkillDef SexSkill;
        public static bool RJWSexperienceIsActive;
    }

    [StaticConstructorOnStartup]
    public static class MainPatch
    {
        static MainPatch()
        {
            ModSettingPatch();
            var harmony = new Harmony("com.github.Creeper-of-Fire.TortureExpand");
            //Harmony.DEBUG = true;
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("[COF_Torture]修改加载中!");
        }

        private static void ModSettingPatch()
        {
            SettingPatch.HygieneNeed = DefDatabase<NeedDef>.GetNamedSilentFail("Hygiene"); //DUBS卫生
            SettingPatch.BladderNeed = DefDatabase<NeedDef>.GetNamedSilentFail("Bladder"); //DUBS膀胱
            if (SettingPatch.HygieneNeed == null && SettingPatch.BladderNeed == null)
            {
                SettingPatch.DubsBadHygieneIsActive = false;
            }
            else
            {
                SettingPatch.DubsBadHygieneIsActive = true;
                Log.Message("[COF_TORTURE]Dubs Bad Hygiene is detected.");
            }

            SettingPatch.DBHThirstNeed = DefDatabase<NeedDef>.GetNamedSilentFail("DBHThirst"); //DUBS口渴
            if (SettingPatch.DBHThirstNeed == null)
            {
                SettingPatch.DubsBadHygieneThirstIsActive = false;
            }
            else
            {
                SettingPatch.DubsBadHygieneThirstIsActive = true;
                Log.Message("[COF_TORTURE]Dubs Bad Hygiene Thirst is detected.");
            }

            SettingPatch.SexNeed = DefDatabase<NeedDef>.GetNamedSilentFail("Sex");
            if (SettingPatch.SexNeed == null)
            {
                SettingPatch.RimJobWorldIsActive = false;
            }
            else
            {
                SettingPatch.RimJobWorldIsActive = true;
                Log.Message("[COF_TORTURE]Rim Job World is detected.");
            }

            SettingPatch.SexSkill = DefDatabase<SkillDef>.GetNamedSilentFail("Sex");
            if (SettingPatch.SexSkill == null)
            {
                SettingPatch.RJWSexperienceIsActive = false;
            }
            else
            {
                SettingPatch.RJWSexperienceIsActive = true;
                Log.Message("[COF_TORTURE]RJW-Sexperience is detected");
            }
        }
    }
}