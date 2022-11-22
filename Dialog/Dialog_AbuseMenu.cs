using System;
using System.Collections.Generic;
using System.Linq;
using COF_Torture.Component;
using COF_Torture.Hediffs;
using UnityEngine;
using Verse;
using static COF_Torture.Dialog.DialogUtility;

namespace COF_Torture.Dialog
{
    public class Dialog_AbuseMenu : Window
    {
        private Pawn pawn;
        private const int EntryHeight = 24;
        private const int IconSize = 36;
        private const int DeleteXSize = 20;
        private const int DefButtonWidth = 60;

        private Vector2 scrollPosition;

        //private const int contextHash = 1275515574;
        private const int SeparatorHeight = 6;

        //private Dictionary<BodyPartGroupDef, List<BodyPartRecord>> allBodyPartDict;
        private Dictionary<BodyPartGroupDef, List<BodyPartRecord>> ableBodyPartGroupsDict =
            new Dictionary<BodyPartGroupDef, List<BodyPartRecord>>();

        private static List<HediffDef> AllHediff = new List<HediffDef>();
        private HediffDef focusHediff;
        private BodyPartGroupDef focusBodyPartGroup;
        private readonly List<ButtonInfo> todoList = new List<ButtonInfo>();

        public override Vector2 InitialSize => new Vector2(700f, 600f);

        public Dialog_AbuseMenu(Pawn pawn)
        {
            this.pawn = pawn;
            //this.doCloseButton = true;
            this.doCloseX = true;
            //this.preventCameraMotion = false;
            Get_AllAbuseHediff();
            this.draggable = true;
            this.resizeable = true;
            //this.allBodyPartDict = GetAllBodyPartGroups(pawn);
            //this.onlyOneOfTypeAllowed = false;
            //this.closeOnClickedOutside = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
        }

        private static void Get_AllAbuseHediff()
        {
            if (!Dialog_AbuseMenu.AllHediff.NullOrEmpty()) return;
            foreach (var hediffDef in DefDatabase<HediffDef>.AllDefs)
            {
                if (hediffDef.hediffClass == typeof(Hediff_AbuseInjury))
                {
                    Dialog_AbuseMenu.AllHediff.Add(hediffDef);
                }
            }
        }

        private Dictionary<BodyPartGroupDef, List<BodyPartRecord>> Get_AbleBodyPartGroups()
        {
            var ableGroupsDict = new Dictionary<BodyPartGroupDef, List<BodyPartRecord>>();
            foreach (var bodyPart in pawn.health.hediffSet.GetNotMissingParts())
            {
                foreach (var group in bodyPart.groups)
                {
                    ableGroupsDict.DictListAdd(group, bodyPart);
                }
            }

            ableGroupsDict = TortureUtility.untieNestedDict(ableGroupsDict);
            //if (!(focusHediff is HediffWithBodyPartGroupsDef hediff_WithBodyPartGroups))
            return ableGroupsDict;

            /*var keys1 = ableGroupsDict.Keys.ToList();
            var keys2 = hediff_WithBodyPartGroups.ableBodyPartGroupDefs;
            var keys3 = keys1.Intersect(keys2);
            var dict = new Dictionary<BodyPartGroupDef, List<BodyPartRecord>>();
            foreach (var group in keys3)
            {
                dict.Add(group, ableGroupsDict[group]);
            }

            if (dict.NullOrEmpty())
                return ableGroupsDict;
            return dict;*/
        }

        /*private static Dictionary<BodyPartGroupDef, List<BodyPartRecord>> Get_AllBodyPartGroups(Pawn pawn)
        {
            var allGroups = new List<BodyPartGroupDef>();
            var allGroupsDict = new Dictionary<BodyPartGroupDef, List<BodyPartRecord>>();

            foreach (var bodyPart in pawn.health.hediffSet.GetNotMissingParts())
            {
                foreach (var group in bodyPart.groups)
                {
                    if (allGroups.NullOrEmpty() || allGroups.Contains(group))
                        allGroupsDict.DictListAdd(group, bodyPart);
                }
            }

            return TortureUtility.untieNestedDict(allGroupsDict);
        }*/

        /*private List<BodyPartRecord> Get_BodyParts()
        {
            var list = new List<BodyPartRecord>();
            if (!this.ableBodyPartGroupsDict.NullOrEmpty() && this.focusBodyPartGroup != null &&
                this.ableBodyPartGroupsDict.ContainsKey(this.focusBodyPartGroup))
            {
                foreach (var bodyPart in this.ableBodyPartGroupsDict[this.focusBodyPartGroup])
                {
                    if (!pawn.health.hediffSet.PartIsMissing(bodyPart))
                        list.Add(bodyPart);
                }
            }

            return list;
        }*/

        /// <summary>
        /// 一级菜单，获得所有可选Hediff
        /// </summary>
        /// <returns>Hediff列表，如果没被选中则处于disable状态</returns>
        private List<ButtonInfo> Buttons_Hediffs()
        {
            return AllHediff.Select(Button_SetHediff).ToList();
        }

        /// <summary>
        /// 二级菜单，获得所有可选BodyPartGroup，由focusHediff定义
        /// </summary>
        /// <returns>BodyPartGroup列表，如果没被选中则处于inactive状态</returns>
        private List<ButtonInfo> Buttons_BodyPartGroups()
        {
            List<ButtonInfo> list = new List<ButtonInfo>();
            foreach (var info in ableBodyPartGroupsDict.Keys)
            {
                list.Add(Button_SetBodyPartGroup(info));
            }

            return list;
        }

        /// <summary>
        /// 三级菜单，获得所有可用BodyPart，由focusHediff和focusBodyPartGroup定义，并且打包为动作
        /// </summary>
        /// <returns>动作列表（由focusHediff和BodyPart参与的动作）</returns>
        private List<ButtonInfo> Buttons_BodyParts()
        {
            if (this.focusHediff == null || focusBodyPartGroup == null ||
                !ableBodyPartGroupsDict.ContainsKey(focusBodyPartGroup))
                return new List<ButtonInfo>();
            else
                return ableBodyPartGroupsDict[focusBodyPartGroup].Select(Button_SetJob).ToList();
        }

        private ButtonInfo Button_SetHediff(HediffDef hediffDef)
        {
            var inactive = this.focusHediff != hediffDef;
            var action = new Action(delegate
            {
                if (this.focusHediff != hediffDef)
                    this.focusHediff = hediffDef;
                else
                    this.focusHediff = null;
            });
            var button = new ButtonInfo(action, hediffDef.label, hediffDef.description, inactived: inactive);
            return button;
        }

        private ButtonInfo Button_SetBodyPartGroup(BodyPartGroupDef bodyPartGroupDef)
        {
            var inactive = this.focusBodyPartGroup != bodyPartGroupDef;
            var disable = false;
            var action = new Action(delegate
            {
                if (this.focusBodyPartGroup == bodyPartGroupDef || this.focusHediff == null)
                    this.focusBodyPartGroup = null;
                else
                    this.focusBodyPartGroup = bodyPartGroupDef;
            });
            if ((focusHediff is HediffWithBodyPartGroupsDef hediff_WithBodyPartGroups))
            {
                disable = true;
                var list = hediff_WithBodyPartGroups.ableBodyPartGroupDefs;
                if (list.Contains(bodyPartGroupDef))
                    disable = false;
            }

            var button = new ButtonInfo(action, bodyPartGroupDef.label, "", disabled: disable, inactived: inactive);
            return button;
        }

        private ButtonInfo Button_SetJob(BodyPartRecord bodyPart)
        {
            var actionToDo = new Action(() => DoAddHediffJob(bodyPart));
            var buttonToDo = new ButtonInfo(actionToDo,
                bodyPart.Label + "," + this.focusHediff.label.Colorize(this.focusHediff.defaultLabelColor), "");
            var actionButton = new Action(delegate
            {
                if (this.focusHediff != null && this.focusBodyPartGroup != null)
                    todoList.Add(buttonToDo);
            });
            var buttonButton = new ButtonInfo(actionButton, bodyPart.Label, "");
            return buttonButton;
        }

        private ButtonInfo Button_ClearToDoList()
        {
            var action = new Action(delegate { this.todoList.Clear(); });
            var button = new ButtonInfo(action, "CT_Button_ClearToDoList".Translate(),
                "CT_Button_ClearToDoListDesc".Translate());
            return button;
        }

        private ButtonInfo Button_Close()
        {
            var action = new Action(delegate
            {
                foreach (var b in this.todoList)
                {
                    b.action();
                }

                this.Close();
            });
            var button = new ButtonInfo(action,
                "CT_CloseWindowForSure".Translate(),
                "CT_CloseWindowForSureDesc".Translate());
            return button;
        }

        private List<ButtonStack> Label_TodoList()
        {
            var labels = new ButtonStacks();
            foreach (var button in todoList)
            {
                labels.Add(button);
            }

            return labels.ToList();
        }

        private void DoAddHediffJob(BodyPartRecord bodyPart)
        {
            var h = HediffMaker.MakeHediff(this.focusHediff, this.pawn, bodyPart);
            pawn.health.AddHediff(h, bodyPart, dinfo: new DamageInfo()); //TODO 加入DInfo，把简单的添加转换为Job
        }

        public override void DoWindowContents(Rect inRect)
        {
            ableBodyPartGroupsDict = Get_AbleBodyPartGroups();
            var buttons_Hediffs = Buttons_Hediffs();
            var buttons_BodyPartGroups = Buttons_BodyPartGroups();
            var buttons_BodyParts = Buttons_BodyParts();
            var label_TodoList = Label_TodoList();

            var buttons_BodyPartGroupsCols = new List<List<ButtonInfo>>();
            var cols = Math.Ceiling((double)buttons_BodyPartGroups.Count / 10);
            var buttons_BodyGroupsColsCount = (int)(buttons_BodyPartGroups.Count / cols);
            for (int i = 0; i < buttons_BodyPartGroups.Count; i += buttons_BodyGroupsColsCount)
            {
                buttons_BodyPartGroupsCols.Add(
                    buttons_BodyPartGroups.Skip(i).Take(buttons_BodyGroupsColsCount).ToList());
            }

            Text.Font = GameFont.Small;

            Rect outRect = new Rect(inRect);
            outRect.yMin += StandardMargin;
            //outRect.yMax -= StandardMargin * 2;
            //outRect.width -= StandardMargin;

            float MaxButtonHeight = EntryHeight;
            float MaxWidthHediff = DefButtonWidth;
            float MaxWidthBodyPart = DefButtonWidth;
            float MaxWidthLabel = DefButtonWidth;
            float DelXSize = DeleteXSize;
            Calc();
            var rectAggregatorButton = new RectAggregator(Rect.zero, this.GetHashCode());
            var rectAggregatorLabel = new RectAggregator(Rect.zero, label_TodoList.GetHashCode());
            DelXSize = MaxButtonHeight;
            GetSpace();
            var width = Mathf.Max(rectAggregatorButton.Rect.width, outRect.width);
            var height = Mathf.Max(rectAggregatorButton.Rect.height, outRect.height);
            //var deltaWidth = this.windowRect.width - width;
            //var deltaHeight = this.windowRect.height - height;
            //Log.Message(deltaHeight + "" + deltaWidth);
            DrawButtons();

            void DrawButtons()
            {
                RectDivider viewRect = new RectDivider(new Rect(0.0f, 0.0f, width, height), this.GetHashCode());
                //Log.Message(this.GetHashCode().ToString());

                var rectHediff = viewRect.NewCol(MaxWidthHediff);
                var rectPartGroupsCols = new List<RectDivider>();
                for (var index = 0; index < buttons_BodyGroupsColsCount; index++)
                    rectPartGroupsCols.Add(viewRect.NewCol(MaxWidthBodyPart));
                var rectParts = viewRect.NewCol(MaxWidthBodyPart);

                Widgets.Label(rectHediff.NewRow(MaxButtonHeight), "CT_UsableAbuseList".Translate());
                foreach (var button in buttons_Hediffs)
                {
                    button.Draw(rectHediff.NewRow(MaxButtonHeight));
                }

                Widgets.DrawLineVertical(rectPartGroupsCols[0].Rect.x - StandardMargin / 2f, viewRect.Rect.y,
                    outRect.height);
                Widgets.Label(rectHediff.NewRow(MaxButtonHeight), "CT_PleaseChooseBodyGroup".Translate());

                for (var index = 0; index < buttons_BodyGroupsColsCount; index++)
                {
                    var Col = buttons_BodyPartGroupsCols[index];
                    var Rect = rectPartGroupsCols[index];
                    foreach (var button in Col)
                    {
                        button.Draw(Rect.NewRow(MaxButtonHeight));
                    }
                }

                Widgets.DrawLineVertical(rectParts.Rect.x - StandardMargin / 2f, viewRect.Rect.y, outRect.height);
                Widgets.Label(rectHediff.NewRow(MaxButtonHeight), "CT_PleaseChooseBody".Translate());

                foreach (var button in buttons_BodyParts)
                {
                    button.Draw(rectParts.NewRow(MaxButtonHeight));
                }

                var rectToDoListOutRect = viewRect.NewCol(rectAggregatorLabel.Rect.width + StandardMargin,
                    HorizontalJustification.Right);
                rectToDoListOutRect = rectToDoListOutRect.NewRow(outRect.height);
                Button_ClearToDoList().Draw(rectToDoListOutRect.NewRow(MaxButtonHeight));
                Button_Close().Draw(rectToDoListOutRect.NewRow(MaxButtonHeight));

                var rectToDoListView =
                    new RectDivider(
                        new Rect(0.0f, 0.0f, rectAggregatorLabel.Rect.width, rectAggregatorLabel.Rect.height),
                        this.GetHashCode());
                Widgets.BeginScrollView(rectToDoListOutRect.Rect, ref this.scrollPosition, rectToDoListView, false);

                Text.Font = GameFont.Small;
                var rectToDoListDelButton = rectToDoListView.NewCol(DelXSize, HorizontalJustification.Right);
                foreach (var stackLabel in label_TodoList)
                {
                    Widgets.Label(rectToDoListView.NewRow(MaxButtonHeight), stackLabel.Label);
                    var rectButton = rectToDoListDelButton.NewRow(MaxButtonHeight);
                    GUI.color = Color.red;
                    if (Widgets.ButtonImage(rectButton, TexButton.DeleteX))
                    {
                        this.todoList.Remove(stackLabel.Button);
                    }

                    GUI.color = Color.white;
                    TooltipHandler.TipRegion(rectButton, "Delete".Translate());
                }

                Text.Font = GameFont.Small;

                Widgets.EndScrollView();
            }

            void Calc()
            {
                CalcWidth("CT_UsableAbuseList".Translate(), ref MaxWidthHediff, GameFont.Small);
                foreach (var button in buttons_Hediffs)
                {
                    CalcHeight(button, ref MaxButtonHeight, GameFont.Small);
                    CalcWidth(button, ref MaxWidthHediff, GameFont.Small);
                }

                Text.Font = GameFont.Small;
                MaxWidthBodyPart = Mathf.Max(MaxWidthBodyPart,
                    Text.CalcSize("CT_PleaseChooseBodyGroup".Translate()).y / 2);
                foreach (var button in buttons_BodyPartGroups)
                {
                    CalcHeight(button, ref MaxButtonHeight);
                    CalcWidth(button, ref MaxWidthBodyPart);
                }

                CalcWidth("CT_PleaseChooseBody".Translate(), ref MaxWidthBodyPart);
                foreach (var bodyPart in pawn.health.hediffSet.GetNotMissingParts())
                {
                    CalcHeight(bodyPart.Label, ref MaxButtonHeight);
                    CalcWidth(bodyPart.Label, ref MaxWidthBodyPart);
                }

                foreach (var stackLabel in label_TodoList)
                {
                    CalcHeight(stackLabel, ref MaxButtonHeight, GameFont.Small);
                    CalcWidth(stackLabel, ref MaxWidthLabel, GameFont.Small);
                }
                //MaxWidthLabel += DelXSize;
            }

            void GetSpace()
            {
                rectAggregatorButton.NewCol(MaxWidthHediff);
                for (var index = 0; index < buttons_BodyGroupsColsCount; index++)
                    rectAggregatorButton.NewCol(MaxWidthBodyPart);
                rectAggregatorButton.NewCol(MaxWidthBodyPart);
                rectAggregatorLabel.NewCol(MaxWidthLabel);
                rectAggregatorLabel.NewCol(DelXSize);
                rectAggregatorLabel.NewCol(StandardMargin);

                var maxRow = Mathf.Max(
                    buttons_Hediffs.Count + 1, buttons_BodyPartGroupsCols[0].Count + 2, buttons_BodyParts.Count + 1
                );
                for (var i = 0; i < maxRow; i++) rectAggregatorButton.NewRow(MaxButtonHeight);
                for (var i = 0; i < label_TodoList.Count + 2; i++) rectAggregatorLabel.NewRow(MaxButtonHeight);
                rectAggregatorButton.NewCol(rectAggregatorLabel.Rect.width);
            }
        }
    }
}