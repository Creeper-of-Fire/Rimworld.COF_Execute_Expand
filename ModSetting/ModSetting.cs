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
            ls.CheckboxLabeled("建筑物默认启用安全模式", ref Setting.isSafe);
            
            ls.GapLine(20f);
            ls.Label("Immortal In Execution".Translate());
            ls.CheckboxLabeled("殖民者在处刑过程中不会死亡", ref Setting.isImmortal);
           
            ls.GapLine(20f);
            ls.Label("Satisfy Hunger And Thirty".Translate());
            ls.CheckboxLabeled("殖民者在刑具上会获得饮食和水的补充", ref Setting.isFeed); 
            
            ls.GapLine(20f);
            ls.Label("Hold Head".Translate());
            ls.CheckboxLabeled("殖民者不会被绞肉机破坏头颅", ref Setting.leftHead); 
            
            ls.GapLine(20f);
            ls.Label("Remove Temp Injuries".Translate());
            ls.CheckboxLabeled("中止处刑时同时移除临时伤口", ref Setting.isRemoveTempInjuries); 
            
            ls.GapLine(20f);
            ls.Label("No Way Back".Translate());
            ls.CheckboxLabeled("处刑一旦开始就无法释放殖民者", ref Setting.isNoWayBack); 
            
            ls.GapLine(20f);
            ls.Label("ControlMenu".Translate());
            ls.CheckboxLabeled("使用菜单管理而非按钮管理殖民者身上的刑具", ref Setting.controlMenuOn); 
            
            ls.GapLine(20f);
            ls.Label("刑具误启动概率（每小时）".Translate());
            ls.Label(Setting.mistakeStartUp.ToStringPercent());
            Setting.mistakeStartUp = ls.Slider(Setting.mistakeStartUp, 0f, 1f);

            ls.GapLine(20f);
            ls.Label("盖子透明度（请重进存档以应用）".Translate());
            ls.Label(Setting.topTransparency.ToStringPercent());
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
        public bool isSafe = true;
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
            Scribe_Values.Look(ref isSafe, "isSafe", true); //建筑物初始是否启用安全模式
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
            isSafe = true;
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