using System;
using System.Collections.Generic;
using System.Linq;
using COF_Torture.Body;
using COF_Torture.Data;
using COF_Torture.Dialog.Menus;
using COF_Torture.Dialog.Units;
using COF_Torture.Hediffs;
using COF_Torture.Utility;
using RimWorld;
using UnityEngine;
using Verse;

namespace COF_Torture.Dialog
{
    public class Dialog_AbuseMenu : Window
    {
        /// <summary>
        /// 窗口所属的角色
        /// </summary>
        private readonly Pawn pawn;

        /// <summary>
        /// 删除按钮的大小
        /// </summary>
        private const int DeleteXSize = 20;

        /// <summary>
        /// 所有可用的身体部分
        /// </summary>
        private Dictionary<string, List<BodyPartRecord>> ableBodyPartGroupsDict =
            new Dictionary<string, List<BodyPartRecord>>();

        /// <summary>
        /// 所有可用的Hediff
        /// </summary>
        private readonly List<MaltreatDef> AllHediff = new List<MaltreatDef>();

        /// <summary>
        /// 选中的Hediff
        /// </summary>
        private MaltreatDef focusHediff;

        /// <summary>
        /// 选中的身体部分
        /// </summary>
        private string focusBodyPartGroup;

        /// <summary>
        /// 预执行的指令（在窗口关闭后执行）
        /// </summary>
        private readonly List<ButtonTextUnit> todoList = new List<ButtonTextUnit>();

        /// <summary>
        /// 菜单
        /// </summary>
        private readonly Menus DialogMenus = new Menus();

        /// <summary>
        /// 刷新屏幕，如果为true就执行刷新
        /// </summary>
        private bool flagShouldRefresh;

        /// <summary>
        ///fixer是角色被捆绑到的建筑物，不应当为空
        /// </summary>
        private readonly ITortureThing fixer;

        /// <summary>
        /// linker是与fixer连接的建筑物，能使得更多刑虐方式有效
        /// </summary>
        private readonly List<string> linker = new List<string>();

        public override Vector2 InitialSize
        {
            get { return new Vector2(700f, 600f); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="fixer"></param>
        public Dialog_AbuseMenu(Pawn pawn, ITortureThing fixer)
        {
            this.pawn = pawn;
            this.pawn.GetPawnData().VirtualParts.RefreshVirtualParts();
            var a = this.pawn.GetPawnData().VirtualParts;
            //ModLog.MessageEach(a.VirtualParts, (o) => o.Key.ToString());
            ModLog.MessageEach(a.VirtualHediffByPart, (o) => o.Key + o.Value.hediffs.Count.ToString());
            //ModLog.MessageEach(a.AllVirtualParts, (o) => o.Key.ToString());
            //PawnExtendUtility.Notify_CheckGenderChange(pawn);
            this.fixer = fixer;
            //this.doCloseButton = true;
            doCloseX = true;
            //this.preventCameraMotion = false;
            draggable = true;
            resizeable = true;
            //this.onlyOneOfTypeAllowed = false;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
            forcePause = true;
            SetAll();

            void SetAll()
            {
                if (fixer is Thing Fixer)
                {
                    //Log.Message("4" + fixer);
                    var comp = Fixer.TryGetComp<CompAffectedByFacilities>();
                    foreach (var thing in comp.LinkedFacilitiesListForReading)
                    {
                        //Log.Message("5" + thing);
                        linker.Add(thing.def.defName);
                    }
                }
                else if (Prefs.DevMode)
                {
                    ModLog.Warning("错误：Dialog_AbuseMenu不应当在不能连接的建筑物上绘制");
                }

                Set_AbleBodyPartGroups();
                Set_AllAbuseHediff();
                IntMenus(DialogMenus);
            }
        }

        private void Set_AllAbuseHediff()
        {
            if (!AllHediff.NullOrEmpty()) return;
            foreach (var maltreatDef in DefDatabase<MaltreatDef>.AllDefs)
            {
                if (maltreatDef.maltreat.enableByBuilding == null ||
                    linker.Contains(maltreatDef.maltreat.enableByBuilding.defName))
                {
                    AllHediff.Add(maltreatDef);
                    //Log.Message("2" + maltreatDef.maltreat.enableByBuilding);
                }

                //Log.Message("1" + maltreatDef);
            }
        }

        /// <summary>
        /// 获得所有的可用身体部件
        /// </summary>
        /// <returns>一个执行了去重的身体部分-身体部件Dictionary</returns>
        private void Set_AbleBodyPartGroups()
        {
            var ablePartGroupsDict = new Dictionary<string, List<BodyPartRecord>>();
            var bodyParts = pawn.health.hediffSet.GetNotMissingParts().ToList();
            bodyParts.AddRange(BodyUtility.GetVirtualParts(pawn).ToList());
            foreach (var bodyPart in bodyParts)
            {
                foreach (var group in bodyPart.groups)
                {
                    if (group.defName == "MiddleFingers" || group.defName == "UpperHead" || group.defName == "Hands")
                        continue;
                    if (group.labelShort != null)
                        ablePartGroupsDict.DictListAdd(group.labelShort, bodyPart);
                    else
                        ablePartGroupsDict.DictListAdd(group.label, bodyPart);
                }
            }

            ableBodyPartGroupsDict = BodyUtility.untieNestedDict(ablePartGroupsDict, "CT_Others".Translate());
        }

        /// <summary>
        /// 一级菜单，获得所有可选Hediff
        /// </summary>
        /// <returns>Hediff列表，如果没被选中则处于disable状态</returns>
        private List<ButtonTextUnit> Buttons_Hediffs()
        {
            return AllHediff.Select(Button_SetHediff).ToList();
        }

        /// <summary>
        /// 二级菜单，获得所有可选BodyPartGroup，由focusHediff定义
        /// </summary>
        /// <returns>BodyPartGroup列表，如果没被选中则处于inactive状态</returns>
        private List<ButtonTextUnit> Buttons_BodyPartGroups()
        {
            List<ButtonTextUnit> list = new List<ButtonTextUnit>();
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
        private List<ButtonTextUnit> Buttons_BodyParts()
        {
            if (focusHediff == null || focusBodyPartGroup == null ||
                !ableBodyPartGroupsDict.ContainsKey(focusBodyPartGroup))
                return new List<ButtonTextUnit>();
            return ableBodyPartGroupsDict[focusBodyPartGroup].Select(Button_SetJob).ToList();
        }

        private ButtonTextUnit Button_SetHediff(MaltreatDef hediff)
        {
            var inactive = focusHediff != hediff;
            var action = new Action(delegate
            {
                if (focusHediff != hediff)
                    focusHediff = hediff;
                else
                    focusHediff = null;
                flagShouldRefresh = true;
            });
            var button = new ButtonTextUnit();
            button.InitInfo(action, hediff.GetLabelAction(),
                hediff.GetDescriptionAction()
                + "\n\n" + "CT_Offer".Translate() + hediff.maltreat.enableByBuilding.label,
                _inactive: inactive);
            return button;
        }

        private ButtonTextUnit Button_SetBodyPartGroup(string bodyPartGroupDef)
        {
            var inactive = focusBodyPartGroup != bodyPartGroupDef;
            var action = new Action(delegate
            {
                if (focusBodyPartGroup == bodyPartGroupDef || focusHediff == null)
                    focusBodyPartGroup = null;
                else
                    focusBodyPartGroup = bodyPartGroupDef;
                flagShouldRefresh = true;
                //Log.Message("" + focusBodyPartGroup);
            });

            var button = new ButtonTextUnit();
            button.InitInfo(action, bodyPartGroupDef, "",
                _disabled: ShouldBodyPartGroupDisabled(bodyPartGroupDef), _inactive: inactive);
            return button;
        }

        private bool ShouldBodyPartGroupDisabled(string bodyGroup)
        {
            bool GroupDisabled = false;
            bool PartDisabled = false;
            if (focusHediff != null)
            {
                var listGroup = focusHediff.maltreat.ableBodyPartGroupDefs;
                if (!listGroup.NullOrEmpty())
                {
                    GroupDisabled = true;
                    foreach (var def in listGroup)
                    {
                        if (def.label == bodyGroup || def.labelShort == bodyGroup)
                        {
                            GroupDisabled = false;
                        }
                    }
                }

                var listPart = focusHediff.maltreat.ableBodyPartDefs;
                if (!listPart.NullOrEmpty())
                {
                    PartDisabled = true;
                    foreach (var def in listPart)
                    {
                        if (ableBodyPartGroupsDict.ContainsKey(bodyGroup) &&
                            ableBodyPartGroupsDict[bodyGroup].Exists(record => record.def.defName == def.defName))
                            PartDisabled = false;
                    }
                }
            }

            return GroupDisabled || PartDisabled;
        }

        private bool ShouldBodyPartDisabled(BodyPartRecord bodyPart)
        {
            bool PartDisabled = false;
            if (focusHediff != null)
            {
                var listPart = focusHediff.maltreat.ableBodyPartDefs;
                if (!listPart.NullOrEmpty())
                {
                    PartDisabled = true;
                    foreach (var def in listPart)
                    {
                        if (bodyPart.def.defName == def.defName)
                            PartDisabled = false;
                    }
                }
            }

            return PartDisabled;
        }

        private ButtonTextUnit Button_SetJob(BodyPartRecord bodyPart)
        {
            //var h = HediffMaker.MakeHediff(focusHediff, pawn, bodyPart);
            var focus = focusHediff;
            var actionToDo = new Action(delegate { DoAddHediffJob(focus, bodyPart); });
            var buttonToDo = new ButtonTextUnit();
            buttonToDo.InitInfo(actionToDo,
                bodyPart.Label + "," + focusHediff.GetLabelAction().Colorize(focusHediff.defaultLabelColor),
                focusHediff.GetDescriptionAction());
            var actionButton = new Action(delegate
            {
                for (var i = 0; i < GenUI.CurrentAdjustmentMultiplier(); i++)
                {
                    if (focusHediff != null && focusBodyPartGroup != null)
                        todoList.Add(buttonToDo);
                }

                flagShouldRefresh = true;
            });
            var buttonButton = new ButtonTextUnit();
            buttonButton.InitInfo(actionButton, bodyPart.Label, "", _disabled: ShouldBodyPartDisabled(bodyPart));
            return buttonButton;
        }

        private ButtonTextUnit Button_ClearToDoList()
        {
            var action = new Action(delegate
            {
                todoList.Clear();
                flagShouldRefresh = true;
            });
            var button = new ButtonTextUnit();
            button.InitInfo(action, "CT_Button_ClearToDoList".Translate(),
                "CT_Button_ClearToDoListDesc".Translate());
            return button;
        }

        private ButtonTextUnit Button_Close()
        {
            var action = new Action(delegate
            {
                foreach (var b in todoList)
                {
                    b.DoAction();
                }

                Close();
            });
            var button = new ButtonTextUnit();
            button.InitInfo(action,
                "CT_CloseWindowForSure".Translate(),
                "CT_CloseWindowForSureDesc".Translate());
            return button;
        }

        private List<LabelWithUnits> Label_TodoList()
        {
            var stacks = UnitStack.stackList(todoList.ListReform<ButtonTextUnit, DialogUnit>());
            ;
            var labels = new List<LabelWithUnits>();

            foreach (var stack in stacks)
            {
                var delButton = new ButtonIconUnit();
                var action = new Action(delegate
                {
                    for (var i = 0; i < GenUI.CurrentAdjustmentMultiplier(); i++)
                    {
                        var buttonInfo = todoList.Find(info => info.label == stack.labelDefault);
                        if (buttonInfo != null)
                            todoList.Remove(buttonInfo);
                    }

                    flagShouldRefresh = true;
                });
                delButton.InitInfo(action, TexButton.DeleteX, Color.red);
                var label = new LabelWithUnits();
                label.InitInfo(delButton, stack.label, stack.desc);
                labels.Add(label);
            }

            return labels;
        }

        private void DoAddHediffJob(MaltreatDef def, BodyPartRecord bodyPart)
        {
            Hediff_COF_Torture_IsAbusing.AddHediff_COF_Torture_IsAbusing(pawn).AddAction(def, bodyPart);
            //TODO 加入另一个人
        }

        public class Menus
        {
            public Dialog_AbuseMenu window;

            public List<DialogUnit> titleHediffs = new List<DialogUnit>();
            public List<DialogUnit> titleBodyGroup = new List<DialogUnit>();
            public List<DialogUnit> titleBody = new List<DialogUnit>();
            public List<DialogUnit> titleTodoList = new List<DialogUnit>();

            public VerticalTitleWithMenu menu_Hediffs;
            public VerticalTitleWithMenu menu_BodyPartGroups;
            public VerticalTitleWithMenu menu_BodyParts;
            public VerticalTitleWithMenu menu_TodoList;

            public List<DialogUnit> buttons_Hediffs;
            public List<DialogUnit> buttons_BodyPartGroups;
            public List<DialogUnit> buttons_BodyParts;
            public List<DialogUnit> label_TodoList;

            public List<VerticalTitleWithMenu> ToMenuList()
            {
                return new List<VerticalTitleWithMenu>
                    { menu_Hediffs, menu_BodyPartGroups, menu_BodyParts, menu_TodoList };
            }

            public List<List<DialogUnit>> ToTitleList()
            {
                return new List<List<DialogUnit>> { titleHediffs, titleBodyGroup, titleBody, titleTodoList };
            }

            public List<List<DialogUnit>> ToUnitList()
            {
                return new List<List<DialogUnit>>
                    { buttons_Hediffs, buttons_BodyPartGroups, buttons_BodyParts, label_TodoList };
            }
        }

        private IEnumerable<DialogUnit> titleHediffs()
        {
            var title1 = new LabelUnit();
            title1.InitInfo("CT_UsableAbuseList".Translate(), "");
            yield return title1;
        }

        private IEnumerable<DialogUnit> titleBodyGroup()
        {
            var title1 = new LabelUnit();
            title1.InitInfo("CT_PleaseChooseBodyGroup".Translate(), "");
            yield return title1;
        }

        private IEnumerable<DialogUnit> titleBody()
        {
            var title1 = new LabelUnit();
            title1.InitInfo("CT_PleaseChooseBody".Translate(), "");
            yield return title1;
        }

        private IEnumerable<DialogUnit> titleTodoList()
        {
            yield return Button_Close();
            yield return Button_ClearToDoList();
        }


        private void IntMenus(Menus menus)
        {
            menus.buttons_Hediffs = Buttons_Hediffs().ListReform<ButtonTextUnit, DialogUnit>();
            menus.buttons_BodyPartGroups = Buttons_BodyPartGroups().ListReform<ButtonTextUnit, DialogUnit>();
            menus.buttons_BodyParts = Buttons_BodyParts().ListReform<ButtonTextUnit, DialogUnit>();
            menus.label_TodoList = Label_TodoList().ListReform<LabelWithUnits, DialogUnit>();
            menus.titleHediffs = titleHediffs().ToList();
            menus.titleBodyGroup = titleBodyGroup().ToList();
            menus.titleBody = titleBody().ToList();
            menus.titleTodoList = titleTodoList().ToList();
            menus.menu_Hediffs =
                new VerticalTitleWithMenu(menus.titleHediffs, new SimpleVerticalMenu(menus.buttons_Hediffs));
            menus.menu_BodyPartGroups =
                new VerticalTitleWithMenu(menus.titleBodyGroup,
                    new SimpleVerticalMenu(menus.buttons_BodyPartGroups, 10));
            menus.menu_BodyParts =
                new VerticalTitleWithMenu(menus.titleBody, new SimpleVerticalMenu(menus.buttons_BodyParts));
            menus.menu_TodoList =
                new VerticalTitleWithMenu(menus.titleTodoList, new SimpleVerticalMenu(menus.label_TodoList));
        }

        private void RefreshMenus(Menus menus)
        {
            if (focusBodyPartGroup != null)
            {
                if (ShouldBodyPartGroupDisabled(focusBodyPartGroup)) //如果focusBodyPartGroup不被允许
                    focusBodyPartGroup = null;
            }

            menus.buttons_Hediffs.Clear();
            menus.buttons_BodyPartGroups.Clear();
            menus.buttons_BodyParts.Clear();
            menus.label_TodoList.Clear();
            menus.buttons_Hediffs.InsertRange(0, Buttons_Hediffs());
            menus.buttons_BodyPartGroups.InsertRange(0, Buttons_BodyPartGroups());
            menus.buttons_BodyParts.InsertRange(0, Buttons_BodyParts());
            menus.label_TodoList.InsertRange(0, Label_TodoList());
            menus.menu_Hediffs.Refresh();
            menus.menu_BodyPartGroups.Refresh();
            menus.menu_BodyParts.Refresh();
            menus.menu_TodoList.Refresh();
        }


        public override void DoWindowContents(Rect inRect)
        {
            if (flagShouldRefresh)
            {
                flagShouldRefresh = false;
                RefreshMenus(DialogMenus);
            }

            Rect outRect = new Rect(inRect);
            outRect.yMin += StandardMargin;
            outRect.yMax -= StandardMargin * 2;
            RectDivider viewRect = new RectDivider(outRect, GetHashCode());
            var menuRects = new List<RectDivider>();
            foreach (var menu in DialogMenus.ToMenuList())
            {
                menu.Draw(viewRect.NewCol(menu.width));
            }
        }
    }
}