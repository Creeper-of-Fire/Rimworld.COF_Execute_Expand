using System;
using UnityEngine;
using Verse;

namespace COF_Torture.ModSetting
{
    //游戏加载时构造实例
    [StaticConstructorOnStartup]
    public class ModSettingMain : Mod
    {
        public ModSetting Setting;
        public static ModSettingMain Instance { get; private set; }
        //private Vector2 scrollPosition = new Vector2();
        public ModSettingMain(ModContentPack content) : base(content)
        {
            Setting = GetSettings<ModSetting>(); //读取本地数据 设置setting中的mod关联
            Instance = this;
        }


        public override string SettingsCategory()
        {
            return "COF_Torture_ModName".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(0, 0, 0.9f*inRect.width, 0.8f * inRect.height);
            Listing_Standard ls = new Listing_Standard();
            //GUI.BeginScrollView(rect, ref Setting.scrollPos, ref rect);
            ls.Begin(inRect);
            Text.Font = GameFont.Tiny;
            if (ls.ButtonText("default".Translate()))
            {
                Setting.InitData();
            } //按钮与监听

            ls.GapLine(5f);
            //ls.Label("Safe Mode Is Default".Translate());
            ls.CheckboxLabeled("CT_Setting_SecurityMode".Translate(), ref Setting.isSecurityMode);
            
            ls.GapLine(5f);
            //ls.Label("Immortal In Execution".Translate());
            ls.CheckboxLabeled("CT_Setting_ImmortalInExecution".Translate(), ref Setting.isImmortal);
           
            ls.GapLine(5f);
            //ls.Label("Satisfy Hunger And Thirty".Translate());
            ls.CheckboxLabeled("CT_Setting_SatisfyNeedsInExecution".Translate(), ref Setting.isFeed); 
            
            ls.GapLine(5f);
            //ls.Label("Hold Head".Translate());
            ls.CheckboxLabeled("CT_Setting_HeadKeptWhenMincer".Translate(), ref Setting.leftHead); 
            
            ls.GapLine(5f);
            //ls.Label("Remove Temp Injuries".Translate());
            ls.CheckboxLabeled("CT_Setting_RemoveTempInjuries".Translate(), ref Setting.isRemoveTempInjuries); 
            
            ls.GapLine(5f);
            //ls.Label("No Way Back".Translate());
            ls.CheckboxLabeled("CT_Setting_NoWayBack".Translate(), ref Setting.isNoWayBack); 
            
            ls.GapLine(5f);
            //ls.Label("ControlMenu".Translate());
            ls.CheckboxLabeled("CT_Setting_ControlMenuOn".Translate(), ref Setting.controlMenuOn); 
            
            ls.GapLine(5f);
            ls.Label("CT_Setting_MistakeStartUp".Translate());
            ls.Label(Setting.mistakeStartUp.ToStringPercent());
            Setting.mistakeStartUp = ls.Slider(Setting.mistakeStartUp, 0f, 1f);

            ls.GapLine(5f);
            ls.Label("CT_Setting_CoverTransparency".Translate());
            ls.Label(Setting.topTransparency.ToStringPercent());
            Setting.topTransparency = ls.Slider(Setting.topTransparency, 0f, 1f);
            
            
            ls.GapLine(5f);
            ls.Label("CT_Setting_ExecutionTimeDesc".Translate());
            ls.Gap(10f);
            
            TextFieldNumericLabeled(ls, "CT_Setting_ExecutionTime".Translate(), ref Setting.executeHours, 0f, 2500f);
            ls.End();
            Text.Font = GameFont.Small;
        }

        private void TextFieldNumericLabeled<T>(Listing_Standard ls, string label, ref T val, float min, float max)
            where T : struct
        {
            string s = val.ToString();
            ls.TextFieldNumericLabeled(label, ref val, ref s, min, max);
        }
    }

    public class ModSetting : ModSettings
    {
        public bool isSecurityMode = false;
        public bool isFeed = true;
        public bool leftHead = true;
        public bool isImmortal = true;
        public bool isRemoveTempInjuries = true;
        public bool isNoWayBack = false;
        //public float testFloat = 1f;
        public int executeHours = 2500;
        public UnityEngine.Vector2 scrollPos = UnityEngine.Vector2.zero;//这玩意哪来的？我怎么一觉醒来多了这行代码？
        public float topTransparency = 1.0f;
        public float mistakeStartUp = 0.0f;
        public bool controlMenuOn = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isSecurityMode, "isSecurityMode", false); //建筑物初始是否启用安全模式
            Scribe_Values.Look(ref isFeed, "isFeed", true);
            Scribe_Values.Look(ref leftHead, "leftHead", true);
            Scribe_Values.Look(ref isImmortal, "isImmortal", true);
            Scribe_Values.Look(ref isRemoveTempInjuries, "isRemoveTempInjuries", true);
            Scribe_Values.Look(ref isNoWayBack, "isNoWayBack", false);
            Scribe_Values.Look(ref controlMenuOn,"controlMenuOn",false);
            Scribe_Values.Look(ref topTransparency, "topTransparency", 1f);
            Scribe_Values.Look(ref mistakeStartUp, "mistakeStartUp", 0f);
            Scribe_Values.Look(ref executeHours, "executeHours", 4);
        }

        public void InitData()
        {
            isSecurityMode = false;
            isFeed = true;
            leftHead = true;
            isImmortal = true;
            isRemoveTempInjuries = true;
            isNoWayBack = false;
            topTransparency = 1f;
            mistakeStartUp = 0f;
            controlMenuOn = false;
            //testInt = 2500;
        }
    }
}