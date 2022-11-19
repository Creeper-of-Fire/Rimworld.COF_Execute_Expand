using System;
using System.Collections.Generic;
using System.Linq;
using COF_Torture.Component;
using COF_Torture.Hediffs;
using COF_Torture.ModSetting;
using COF_Torture.Things;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace COF_Torture
{
    public static class DialogUtility
    {
        public static IEnumerable<IWithGiver> AllTortureHediff(Pawn pawn)
        {
            foreach (var h in pawn.health.hediffSet.hediffs)
            {
                if (h is IWithGiver hg && hg.Giver != null)
                {
                    yield return hg;
                }
            }
        }

        /*public class DoubleKeyDictionary<Key1, Key2, Value>
        {
            public Dictionary<Key1, Dictionary<Key2, Value>> multiDict =
                new Dictionary<Key1, Dictionary<Key2, Value>>();

            public void SetSet(Key1 key1, Key2 key2, Value value)
            {
                if (multiDict.ContainsKey(key1))
                {
                    var dict2 = multiDict[key1];
                    if (dict2.ContainsKey(key2))
                        dict2[key2] = value;
                    else
                        dict2.Add(key2, value);
                }
                else
                {
                    var dict2 = new Dictionary<Key2, Value>();
                    dict2.Add(key2, value);
                    multiDict.Add(key1, dict2);
                }
            }

            public void SetAdd(Key1 key1, Key2 key2, Value value)
            {
                if (multiDict.ContainsKey(key1))
                {
                    var dict2 = multiDict[key1];
                    dict2.Add(key2, value);
                }
                else
                {
                    var dict2 = new Dictionary<Key2, Value>();
                    dict2.Add(key2, value);
                    multiDict.Add(key1, dict2);
                }
            }

            public void AddAdd(Key1 key1, Key2 key2, Value value)
            {
                var dict2 = new Dictionary<Key2, Value>();
                dict2.Add(key2, value);
                multiDict.Add(key1, dict2);
            }

            public Value Get(Key1 key1, Key2 key2)
            {
                Value value = default;
                if (key1 != null)
                    if (key2 != null)
                        value = multiDict[key1][key2];
                return value;
            }

            public int Count => multiDict.Count;
        }

        public class ButtonDict
        {
            public Dictionary<string, ButtonInfo> Dict = new Dictionary<string, ButtonInfo>();
        }*/

        public static Dictionary<Thing, List<Hediff>> GetTortureManageMenuColumns(this Pawn pawn)
        {
            var dict = new Dictionary<Thing, List<Hediff>>();
            foreach (var h in pawn.health.hediffSet.hediffs)
            {
                if (!(h is Hediff_WithGiver hediff)) continue;
                Thing key1 = hediff.Giver;
                Hediff key2 = hediff;
                if (key1 == null)
                {
                    if (Prefs.DevMode)
                        Log.Error("[COF_Torture]" + hediff + "是Hediff_WithGiver类型，但是Giver为空。本报错为开发者信息，这意味着游戏可能没有出错。");
                    continue;
                }

                if (dict.ContainsKey(key1))
                    dict[key1].Add(key2);
                else
                {
                    var list = new List<Hediff> { key2 };
                    dict.Add(key1, list);
                }
            }

            return dict;
        }

        public class ButtonInfo
        {
            public Action action;
            public string label;
            public bool disabled;
            public Command_Action CommandAction;

            public ButtonInfo(Command_Action command)
            {
                action = command.action;
                disabled = command.disabled;
                label = command.Label;
                CommandAction = command;
            }
        }

        public static List<ButtonInfo> GetButtons(object ButtonOwner)
        {
            if (ButtonOwner is Hediff h)
                return GetButtons(h);
            if (ButtonOwner is Thing t)
                return GetButtons(t);
            Log.Error("[COF_Torture]" + ButtonOwner + "拥有错误的类型" + ButtonOwner.GetType());
            return new List<ButtonInfo>();
        }

        public static List<ButtonInfo> GetButtons(this Hediff ButtonOwner)
        {
            if (ButtonOwner is Hediff_Fixed hf)
                return GetButtons(hf);
            var hcEi = ButtonOwner.TryGetComp<HediffComp_ExecuteIndicator>();
            if (hcEi != null)
                return GetButtons(hcEi);
            var hcSas = ButtonOwner.TryGetComp<HediffComp_SwitchAbleSeverity>();
            if (hcSas != null)
                return GetButtons(hcSas);
            return new List<ButtonInfo>();
        }

        public static List<ButtonInfo> GetButtons(this Thing ButtonOwner)
        {
            if (ButtonOwner is Building_TortureBed btb)
                return GetButtons(btb);
            return new List<ButtonInfo>();
        }

        public static List<ButtonInfo> GetButtons(this Building_TortureBed ButtonOwner)
        {
            var com = new Command_Action();
            if (ButtonOwner.isSafe)
            {
                com.defaultLabel = "CT_isSafe".Translate() + "Enabled".Translate();
            }
            else
            {
                com.defaultLabel = "CT_isSafe".Translate() + "Disabled".Translate();
            }

            com.defaultDesc = "CT_isSafeDesc".Translate();
            com.hotKey = KeyBindingDefOf.Misc4;
            com.icon = GizmoIcon.texSkull;
            com.action = () => ButtonOwner.isSafe = !ButtonOwner.isSafe;

            return new List<ButtonInfo>() { new ButtonInfo(com) };
        }

        public static List<ButtonInfo> GetButtons(this Hediff_Fixed hediff)
        {
            var com = new Command_Action();
            com.defaultLabel = "CT_Release".Translate();
            com.defaultDesc = "CT_Release_BondageBed".Translate();
            com.hotKey = KeyBindingDefOf.Misc5;
            com.icon = GizmoIcon.texPodEject;
            com.action = hediff.GiverAsInterface.ReleaseVictim;
            return new List<ButtonInfo>() { new ButtonInfo(com) };
        }

        public static List<ButtonInfo> GetButtons(this HediffComp_ExecuteIndicator hediff)
        {
            var com = new Command_Action();
            if (!hediff.isInProgress)
            {
                com.defaultLabel = "CT_doExecute".Translate();
                com.defaultDesc = "CT_doExecute".Translate();
                com.hotKey = KeyBindingDefOf.Misc5;
                com.icon = GizmoIcon.texSkull;
                com.action = hediff.StartProgress;
            }
            else
            {
                com.defaultLabel = "CT_stopExecute".Translate();
                com.defaultDesc = "CT_stopExecute".Translate();
                com.hotKey = KeyBindingDefOf.Misc5;
                com.icon = GizmoIcon.texSkull;
                com.action = hediff.StopProgress;
            }

            return new List<ButtonInfo>() { new ButtonInfo(com) };
        }

        public static List<ButtonInfo> GetButtons(this HediffComp_SwitchAbleSeverity hediffComp)
        {
            var stageUp = new Command_Action();
            stageUp.defaultLabel = "CT_stageUp".Translate();
            stageUp.defaultDesc = "CT_stageUp_desc".Translate();
            stageUp.icon = ContentFinder<Texture2D>.Get("COF_Torture/UI/SwitchStage");
            stageUp.action = hediffComp.upStage;
            if (hediffComp.stageLimit >= hediffComp.stageMax)
                stageUp.Disable();
            var stageDown = new Command_Action();
            stageDown.defaultLabel = "CT_stageDown".Translate();
            stageDown.defaultDesc = "CT_stageDown_desc".Translate();
            stageDown.icon = ContentFinder<Texture2D>.Get("COF_Torture/UI/SwitchStage");
            stageDown.action = hediffComp.downStage;
            if (hediffComp.stageLimit <= 0)
                stageDown.Disable();

            return new List<ButtonInfo>() { new ButtonInfo(stageDown), new ButtonInfo(stageUp) };
        }
    }

    public class Dialog_TortureThingManager : Window
    {
        private Pawn pawn;
        private Vector2 scrollPosition;
        private const int EntryHeight = 24;
        private const int IconSize = 48;
        private const int DefButtonWidth = 40;
        private const int contextHash = 1279515574;
        private const int SeparatorHeight = 7;
        private Dictionary<Thing, List<Hediff>> menuColumnDict;

        public override Vector2 InitialSize => new Vector2(520f, 500f);

        public Dialog_TortureThingManager(Pawn pawn)
        {
            this.pawn = pawn;
            this.doCloseButton = true;
            this.doCloseX = true;
            this.closeOnClickedOutside = true;
            //this.absorbInputAroundWindow = true;
        }

        public static void CalcButtonWidth(object menuColumn, ref float maxButtonWidth)
        {
            Text.Font = GameFont.Small;
            foreach (var button in DialogUtility.GetButtons(menuColumn))
            {
                maxButtonWidth = Mathf.Max(maxButtonWidth, Text.CalcSize(button.label).x);
            }

            Text.Font = GameFont.Medium;
        }

        public static void CalcHeight(object menuColumn, ref float maxHeight, float width)
        {
            maxHeight = Mathf.Max(maxHeight, Text.CalcHeight(GetLabel(menuColumn), width));
        }

        private static string GetLabel(object menuColumn)
        {
            if (menuColumn is Thing thingColumn)
                return thingColumn.Label;
            if (menuColumn is Hediff hediffColumn)
                return hediffColumn.Label;
            Log.Error("[COF_Torture]" + menuColumn + "中没有发现对应的Label");
            return "";
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            menuColumnDict = pawn.GetTortureManageMenuColumns();
            if (menuColumnDict.Count <= 0)
            {
                Listing_Standard ls = new Listing_Standard();
                ls.Begin(inRect);
                ls.Label("CT_TortureThingManagerNull".Translate());
                ls.End();
            }

            Rect outRect = new Rect(inRect);
            outRect.yMin += StandardMargin;
            outRect.yMax -= StandardMargin * 2;
            outRect.width -= StandardMargin;
            float maxHeightGiver = Mathf.Max(EntryHeight, IconSize);
            float maxHeightHediff = EntryHeight;
            float maxButtonWidth = DefButtonWidth;
            float maxWidth = maxButtonWidth;
            RectAggregator rectAggregator = new RectAggregator(Rect.zero, contextHash);
            //float width = rectAggregator.Rect.width;

            foreach (var menuColumns in this.menuColumnDict)
            {
                CalcButtonWidth(menuColumns.Key, ref maxButtonWidth);
                foreach (var menuColumn in menuColumns.Value)
                {
                    CalcButtonWidth(menuColumn, ref maxButtonWidth);
                }
            }

            maxButtonWidth += SeparatorHeight; //与边缘的间距
            //maxHeightGiver += SeparatorHeight;
            rectAggregator.NewCol(maxButtonWidth); //按钮宽度
            rectAggregator.NewCol(IconSize); //图片宽度
            float width = outRect.width - rectAggregator.Rect.width;
            foreach (var menuColumns in this.menuColumnDict)
            {
                CalcHeight(menuColumns.Key, ref maxHeightGiver, width);
                foreach (var menuColumn in menuColumns.Value)
                {
                    CalcHeight(menuColumn, ref maxHeightHediff, width);
                }
            }

            foreach (var menuColumns in this.menuColumnDict)
            {
                rectAggregator.NewRow(maxHeightGiver);
                foreach (var menuColumn in menuColumns.Value)
                {
                    rectAggregator.NewRow(maxHeightHediff);
                }

                rectAggregator.NewRow(SeparatorHeight);
            }

            float height = rectAggregator.Rect.height;
            RectDivider viewRect =
                new RectDivider(new Rect(0.0f, 0.0f, outRect.width - StandardMargin, height), contextHash);
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, (Rect)viewRect);

            foreach (var menuColumns in this.menuColumnDict)
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                DrawColumn(menuColumns.Key, menuColumns.Key.DescriptionDetailed, maxHeightGiver,
                    menuColumns.Key.DrawColor);
                foreach (var menuColumn in menuColumns.Value)
                {
                    DrawColumn(menuColumn, menuColumn.Description, maxHeightHediff, menuColumn.LabelColor,
                        font: GameFont.Small);
                }

                Text.Anchor = TextAnchor.UpperLeft;
                Rect rect = viewRect.NewRow(SeparatorHeight).Rect;
                Widgets.DrawLineHorizontal(rect.x, rect.y + rect.height / 2f, rect.width);
            }

            Widgets.EndScrollView();

            void DrawColumn(object menuColumn, string floatMenuText, float maxHeight, Color color = default,
                GameFont font = GameFont.Medium)
            {
                Text.Font = font;
                if (color == default)
                    color = Color.white;
                GUI.color = color;
                RectDivider columnRect = viewRect.NewRow(maxHeight);
                Widgets.DrawHighlightIfMouseover((Rect)columnRect);
                if (Mouse.IsOver(columnRect))
                {
                    TooltipHandler.TipRegion(columnRect, floatMenuText);
                }

                if (menuColumn is Thing thing)
                {
                    var iconRect = columnRect.NewCol(IconSize);
                    Widgets.ThingIcon((Rect)iconRect, thing);
                }

                Widgets.Label((Rect)columnRect, GetLabel(menuColumn));
                GUI.color = Color.white;
                DrawButton(menuColumn, columnRect);
                Text.Font = GameFont.Medium;
            }

            void DrawButton(object menuColumn, RectDivider columnRect)
            {
                Text.Font = GameFont.Small;
                foreach (var button in DialogUtility.GetButtons(menuColumn))
                {
                    if (button.disabled) GUI.color = Color.grey;
                    if (Widgets.ButtonText(
                            (Rect)columnRect.NewCol(maxButtonWidth, HorizontalJustification.Right),
                            (string)button.label))
                    {
                        button.action();
                        SoundDefOf.Click.PlayOneShotOnCamera();
                    }

                    GUI.color = Color.white;
                }

                Text.Font = GameFont.Medium;
            }
        }
    }
}