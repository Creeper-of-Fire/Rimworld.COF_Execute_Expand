using UnityEngine;
using Verse;
using System.Collections;

namespace COF_Torture.ModSetting
{
    //游戏加载时构造实例
    [StaticConstructorOnStartup]
    public class ModSettingMain : Mod
    {
        public ModSetting Setting;
        public static ModSettingMain Instance { get; private set; }

        public ModSettingMain(ModContentPack content) : base(content)
        {
            Setting = GetSettings<ModSetting>(); //读取本地数据 设置setting中的mod关联
            Instance = this;
        }


        public override string SettingsCategory()
        {
            return "COF的更多酷刑模组";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new Rect(0, 0, 0.9f * inRect.width, 0.8f * inRect.height);
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);
            if (ls.ButtonText("恢复默认".Translate()))
            {
                Setting.InitData();
            } //按钮与监听

            ls.GapLine(20f);
            ls.Label("死亡模式".Translate());
            ls.CheckboxLabeled("殖民者不会因为酷刑死亡", ref Setting.isSafe, "殖民者不会因为酷刑死亡");
            ls.GapLine(20f);
            ls.Label("自动补充".Translate());
            ls.CheckboxLabeled("殖民者在刑具上会获得饮食和水的补充", ref Setting.isFeed, "殖民者在刑具上会获得饮食和水的补充");
            /*ls.GapLine(20f);
            Text.Font = GameFont.Medium;
            ls.Label("浮点测试".Translate());
            ls.Label(Setting.testFloat.ToString());
            Setting.testFloat = ls.Slider(Setting.testFloat, 100f, 300f);
            Text.Font = GameFont.Small;
            ls.GapLine(20f);
            Text.Font = GameFont.Medium;
            ls.Label("整型测试".Translate());
            ls.Gap(10f);
            Text.Font = GameFont.Small;
            TextFieldNumericLabeled(ls, "整型测试", ref Setting.testInt, 0f, 30f);*/
            ls.End();
        }

        /*private void TextFieldNumericLabeled<T>(Listing_Standard ls, string label, ref T val, float min, float max)
            where T : struct
        {
            string s = val.ToString();
            ls.TextFieldNumericLabeled(label, ref val, ref s, min, max);
        }*/
    }

    public class ModSetting : ModSettings
    {
        #region param

        public bool isSafe = true;
        public bool isFeed = true;
        //public float testFloat = 1f;
        //public int testInt = 2500;
        public UnityEngine.Vector2 scrollPos = UnityEngine.Vector2.zero;

        #endregion

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isSafe, "isSafe", true);
            Scribe_Values.Look(ref isFeed, "isFeed", true);
            //Scribe_Values.Look(ref testFloat, "TestFloat", 1f);
            //Scribe_Values.Look(ref testInt, "TestInt", 2500);
        }

        public void InitData()
        {
            isSafe = true;
            isFeed = true;
            //testFloat = 1f;
            //testInt = 2500;
        }
    }
}