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
            public string desc;
            public Command Command;

            public ButtonInfo(Command_Action command)
            {
                action = command.action;
                disabled = command.disabled;
                label = command.Label;
                desc = command.Desc;
                if (desc == "No description.")
                {
                    desc = "";
                }

                Command = command;
            }

            public ButtonInfo(Command_Toggle command)
            {
                action = command.toggleAction;
                disabled = command.disabled;
                label = command.Label;
                desc = command.Desc;
                if (desc == "No description.")
                {
                    desc = "";
                }

                Command = command;
            }
        }

        public static List<ButtonInfo> TryGetButtons(object ButtonOwner)
        {
            try
            {
                List<ButtonInfo> buttons = new List<ButtonInfo>();
                if (ButtonOwner is Hediff h)
                    buttons = GetButtons(h);
                else if (ButtonOwner is Thing t)
                    buttons = GetButtons(t);
                return buttons;
            }
            catch
            {
                Log.Error("[COF_Torture]" + ButtonOwner + "拥有错误的类型或其他错误" + ButtonOwner.GetType());
            }

            return new List<ButtonInfo>();
        }

        private static IEnumerable<ButtonInfo> getButtonInfo(IEnumerable<Command> b)
        {
            foreach (var button in b)
            {
                switch (button)
                {
                    case Command_Action ca:
                        yield return new ButtonInfo(ca);
                        break;
                    case Command_Toggle ct:
                        yield return new ButtonInfo(ct);
                        break;
                }
            }
        }

        private static List<ButtonInfo> GetButtons(Hediff ButtonOwner)
        {
            if (ButtonOwner is Hediff_Fixed hf)
                return getButtonInfo(Gizmo_ReleaseBondageBed(hf)).ToList();
            var hcEi = ButtonOwner.TryGetComp<HediffComp_ExecuteIndicator>();
            if (hcEi != null)
                return getButtonInfo(Gizmo_StartAndStopExecute(hcEi)).ToList();
            var hcSas = ButtonOwner.TryGetComp<HediffComp_SwitchAbleSeverity>();
            if (hcSas != null)
                return getButtonInfo(Gizmo_StageUpAndDown(hcSas)).ToList();
            return new List<ButtonInfo>();
        }

        private static List<ButtonInfo> GetButtons(Thing ButtonOwner)
        {
            if (ButtonOwner is Building_TortureBed btb)
                return getButtonInfo(Gizmo_SafeMode(btb)).ToList();
            return new List<ButtonInfo>();
        }

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

    public class Dialog_TortureThingManager : Window
    {
        private Pawn pawn;
        private Vector2 scrollPosition;
        private const int EntryHeight = 24;
        private const int IconSize = 36;
        private const int DefButtonWidth = 40;
        private const int contextHash = 1279515574;
        private const int SeparatorHeight = 6;
        private Dictionary<Thing, List<Hediff>> menuColumnDict;

        public override Vector2 InitialSize => new Vector2(520f, 500f);

        public Dialog_TortureThingManager(Pawn pawn)
        {
            this.pawn = pawn;
            this.doCloseButton = true;
            this.doCloseX = true;
            this.preventCameraMotion = false;
            this.draggable = true;
            this.resizeable = true;
            //this.onlyOneOfTypeAllowed = false;
            //this.closeOnClickedOutside = true;
            //this.absorbInputAroundWindow = true;
        }

        private static void CalcButtonWidth(object menuColumn, ref float maxButtonWidth)
        {
            Text.Font = GameFont.Small;
            foreach (var button in DialogUtility.TryGetButtons(menuColumn))
            {
                maxButtonWidth = Mathf.Max(maxButtonWidth, Text.CalcSize(button.label).x);
            }

            Text.Font = GameFont.Medium;
        }

        private static void CalcWidth(object menuColumn, ref float maxWidth, GameFont font = GameFont.Medium)
        {
            Text.Font = font;
            //maxHeight = Mathf.Max(maxHeight, Text.CalcHeight(GetLabel(menuColumn), width));
            maxWidth = Mathf.Max(maxWidth, Text.CalcSize(GetLabel(menuColumn)).x);
            Text.Font = GameFont.Medium;
        }
        private static void CalcHeight(object menuColumn, ref float maxHeight, GameFont font = GameFont.Medium)
        {
            Text.Font = font;
            maxHeight = Mathf.Max(maxHeight, Text.CalcSize(GetLabel(menuColumn)).y);
            Text.Font = GameFont.Medium;
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
            var pawns = Find.Selector.SelectedPawns;
            if (!pawns.NullOrEmpty() && !pawns.Contains(pawn))
            {
                var pawn_temp = pawns[0];
                foreach (var gizmo in pawn_temp.GetGizmos())
                {
                    if (gizmo is Command_Action command)
                    {
                        if (command.defaultLabel == "CT_TortureThingManager".Translate())
                        {
                            pawn = pawn_temp;
                        }
                    }
                }
            }
            //LocalTargetInfo.Invalid.
            //TODO 保存窗口大小

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
            outRect.width += StandardMargin;
            float maxHeightGiver = Mathf.Max(EntryHeight, IconSize);
            float maxHeightHediff = EntryHeight;
            float maxButtonWidth = DefButtonWidth;
            float maxTextWidth = DefButtonWidth;
            RectAggregator rectAggregator = new RectAggregator(Rect.zero, contextHash);

            CalcHeight(pawn, ref maxHeightHediff);
            foreach (var menuColumns in this.menuColumnDict)
            {
                CalcWidth(menuColumns.Key,ref maxTextWidth);
                CalcHeight(menuColumns.Key, ref maxHeightGiver);
                CalcButtonWidth(menuColumns.Key, ref maxButtonWidth);
                foreach (var menuColumn in menuColumns.Value)
                {
                    CalcWidth(menuColumns.Key,ref maxTextWidth, font: GameFont.Small);
                    CalcHeight(menuColumn, ref maxHeightHediff, font: GameFont.Small);
                    CalcButtonWidth(menuColumn, ref maxButtonWidth);
                }
            }

            //maxButtonWidth += SeparatorHeight; //与边缘的间距
            //maxHeightGiver += SeparatorHeight;
            rectAggregator.NewCol(maxButtonWidth); //按钮宽度
            rectAggregator.NewCol(maxTextWidth);
            rectAggregator.NewCol(IconSize); //图片宽度

            rectAggregator.NewRow(maxHeightHediff);
            rectAggregator.NewRow(SeparatorHeight);
            foreach (var menuColumns in this.menuColumnDict)
            {
                rectAggregator.NewRow(maxHeightGiver);
                for (var index = menuColumns.Value.Count - 1; index >= 0; index--)
                {
                    rectAggregator.NewRow(maxHeightHediff);
                }
                rectAggregator.NewRow(SeparatorHeight);
            }

            float height = rectAggregator.Rect.height;
            float width = rectAggregator.Rect.width;
            width = Mathf.Max(width, outRect.width - StandardMargin);
            RectDivider viewRect =
                new RectDivider(new Rect(0.0f, 0.0f, width, height), contextHash);

            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect);

            Text.Anchor = TextAnchor.MiddleLeft;
            DrawColumn(pawn, maxHeightHediff, isDrawButton: false, font: GameFont.Medium);
            Text.Anchor = TextAnchor.UpperLeft;

            Rect rect1 = viewRect.NewRow(SeparatorHeight * 0.5f).Rect;
            Widgets.DrawLineHorizontal(rect1.x, rect1.y + rect1.height / 2f, rect1.width);

            foreach (var menuColumns in this.menuColumnDict)
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                DrawColumn(menuColumns.Key, maxHeightGiver, menuColumns.Key.DescriptionDetailed,
                    menuColumns.Key.DrawColor, font: GameFont.Medium);
                foreach (var menuColumn in menuColumns.Value)
                {
                    DrawColumn(menuColumn, maxHeightHediff, menuColumn.Description, menuColumn.LabelColor,
                        font: GameFont.Small);
                }

                Text.Anchor = TextAnchor.UpperLeft;
                Rect rect2 = viewRect.NewRow(SeparatorHeight).Rect;
                Widgets.DrawLineHorizontal(rect2.x, rect2.y + rect2.height / 2f, rect2.width);
            }

            Widgets.EndScrollView();


            void DrawColumn(object menuColumn, float maxHeight, string floatMenuText = "", Color color = default,
                GameFont font = GameFont.Medium, bool isDrawButton = true)
            {
                Text.Font = font;
                if (color == default)
                    color = Color.white;
                GUI.color = color;
                RectDivider columnRect = viewRect.NewRow(maxHeight);
                Widgets.DrawHighlightIfMouseover(columnRect);

                if (Mouse.IsOver(columnRect) && !floatMenuText.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(columnRect, floatMenuText);
                }

                if (menuColumn is Thing thing)
                {
                    Widgets.DrawLightHighlight(columnRect);
                    var iconRect = columnRect.NewCol(IconSize);
                    Widgets.ThingIcon(iconRect, thing);
                }

                Widgets.Label(columnRect, GetLabel(menuColumn));//.Truncate(width-maxButtonWidth));
                GUI.color = Color.white;
                if (isDrawButton)
                    DrawButton(menuColumn, columnRect);
                Text.Font = GameFont.Medium;
            }

            void DrawButton(object menuColumn, RectDivider columnRect)
            {
                Text.Font = GameFont.Tiny;
                foreach (var button in DialogUtility.TryGetButtons(menuColumn))
                {
                    string ButtonTipText = button.desc;
                    if (button.disabled)
                    {
                        GUI.color = Color.grey;
                        ButtonTipText = button.Command.disabledReason;
                    }

                    var rectButton = columnRect.NewCol(maxButtonWidth, HorizontalJustification.Right);
                    if (Widgets.ButtonText(
                            rectButton,
                            button.label))
                    {
                        button.action();
                        SoundDefOf.Click.PlayOneShotOnCamera();
                    }

                    if (Mouse.IsOver(rectButton) && !ButtonTipText.NullOrEmpty())
                    {
                        TooltipHandler.TipRegion(rectButton, ButtonTipText);
                    }

                    GUI.color = Color.white;
                }

                Text.Font = GameFont.Medium;
            }
        }
    }
}