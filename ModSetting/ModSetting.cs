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
            ls.Label("Safe Mode Is Default".Translate());
            ls.CheckboxLabeled("建筑物默认启用安全模式", ref Setting.isSafe, "建筑物默认启用安全模式");
            
            ls.GapLine(20f);
            ls.Label("Immortal in execution".Translate());
            ls.CheckboxLabeled("殖民者在处刑过程中不会死亡", ref Setting.isImmortal, "建筑物默认启用安全模式");
           
            ls.GapLine(20f);
            ls.Label("Satisfy Hunger And Thirty".Translate());
            ls.CheckboxLabeled("殖民者在刑具上会获得饮食和水的补充", ref Setting.isFeed, "殖民者在刑具上会获得饮食和水的补充"); 
            
            ls.GapLine(20f);
            ls.Label("Hold Head".Translate());
            ls.CheckboxLabeled("殖民者不会被绞肉机破坏头颅", ref Setting.leftHead, "殖民者不会被绞肉机破坏头颅"); 
            
            ls.GapLine(20f);
            ls.Label("remove temp injuries".Translate());
            ls.CheckboxLabeled("中止处刑时同时移除临时伤口", ref Setting.isRemoveTempInjuries, "中止处刑时同时移除临时伤口"); 

            ls.GapLine(20f);
            Text.Font = GameFont.Medium;
            ls.Label("盖子透明度（请重进存档以应用）".Translate());
            ls.Label(Setting.topTransparency.ToString());
            Setting.topTransparency = ls.Slider(Setting.topTransparency, 0f, 1f);
            
            Text.Font = GameFont.Small;
            ls.GapLine(20f);
            Text.Font = GameFont.Medium;
            ls.Label("处刑时间".Translate());
            ls.Gap(10f);
            Text.Font = GameFont.Small;
            TextFieldNumericLabeled(ls, "自由调整处刑时间，单位为【游戏中的小时】，一小时为2500tick", ref Setting.executeHours, 0f, 2500f);
            ls.End();
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
        #region param

        public bool isSafe = true;
        public bool isFeed = true;
        public bool leftHead = true;
        public bool isImmortal = true;
        public bool isRemoveTempInjuries = true;
        //public float testFloat = 1f;
        public int executeHours = 2500;
        public UnityEngine.Vector2 scrollPos = UnityEngine.Vector2.zero;//这玩意哪来的？我怎么一觉醒来多了这行代码？
        public float topTransparency = 1.0f;

        #endregion

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isSafe, "isSafe", true); //建筑物初始是否启用安全模式
            Scribe_Values.Look(ref isFeed, "isFeed", true);
            Scribe_Values.Look(ref leftHead, "leftHead", true);
            Scribe_Values.Look(ref isImmortal, "isImmortal", true);
            Scribe_Values.Look(ref isRemoveTempInjuries, "isRemoveTempInjuries", true);
            Scribe_Values.Look(ref topTransparency, "topTransparency", 1f);
            Scribe_Values.Look(ref executeHours, "executeHours", 4);
        }

        public void InitData()
        {
            isSafe = true;
            isFeed = true;
            leftHead = true;
            isImmortal = true;
            isRemoveTempInjuries = true;
            topTransparency = 1f;
            //testInt = 2500;
        }
    }
}