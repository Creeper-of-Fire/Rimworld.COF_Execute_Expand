using System.Collections.Generic;
using System.Linq;
using COF_Torture.Component;
using COF_Torture.Hediffs;
using COF_Torture.Things;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using static COF_Torture.Dialog.DialogUtility;

namespace COF_Torture.Dialog
{
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

        private static List<ButtonInfo> TryGetButtons(object ButtonOwner)
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

        private static List<ButtonInfo> GetButtons(Hediff ButtonOwner)
        {
            if (ButtonOwner is Hediff_Fixed hf)
                return getButtonInfo(hf.Gizmo_ReleaseBondageBed()).ToList();
            var hcEi = ButtonOwner.TryGetComp<HediffComp_ExecuteIndicator>();
            if (hcEi != null)
                return getButtonInfo(hcEi.Gizmo_StartAndStopExecute()).ToList();
            var hcSas = ButtonOwner.TryGetComp<HediffComp_SwitchAbleSeverity>();
            if (hcSas != null)
                return getButtonInfo(hcSas.Gizmo_StageUpAndDown()).ToList();
            return new List<ButtonInfo>();
        }

        private static List<ButtonInfo> GetButtons(Thing ButtonOwner)
        {
            if (ButtonOwner is Building_TortureBed btb)
                return getButtonInfo(btb.Gizmo_SafeMode()).ToList();
            return new List<ButtonInfo>();
        }

        private static void CalcButtonWidth(object menuColumn, ref float maxButtonWidth)
        {
            Text.Font = GameFont.Small;
            foreach (var button in TryGetButtons(menuColumn))
            {
                maxButtonWidth = Mathf.Max(maxButtonWidth, Text.CalcSize(button.label).x);
            }

            Text.Font = GameFont.Medium;
        }

        private static Dictionary<Thing, List<Hediff>> GetTortureManageMenuColumns(Pawn pawn)
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

                dict.DictListAdd(key1, key2);
            }

            return dict;
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
            menuColumnDict = GetTortureManageMenuColumns(pawn);
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
                CalcWidth(menuColumns.Key, ref maxTextWidth);
                CalcHeight(menuColumns.Key, ref maxHeightGiver);
                CalcButtonWidth(menuColumns.Key, ref maxButtonWidth);
                foreach (var menuColumn in menuColumns.Value)
                {
                    CalcWidth(menuColumns.Key, ref maxTextWidth, font: GameFont.Small);
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

                Widgets.Label(columnRect, GetLabel(menuColumn)); //.Truncate(width-maxButtonWidth));
                GUI.color = Color.white;
                if (isDrawButton)
                    DrawButton(menuColumn, columnRect);
                Text.Font = GameFont.Medium;
            }

            void DrawButton(object menuColumn, RectDivider columnRect)
            {
                foreach (var button in TryGetButtons(menuColumn))
                {
                    var rectButton = columnRect.NewCol(maxButtonWidth, HorizontalJustification.Right);
                    button.Draw(rectButton,gameFont:GameFont.Tiny);
                }

                Text.Font = GameFont.Medium;
            }
        }

        
    }
}