using System;
using System.Collections.Generic;
using System.Linq;
using COF_Torture.Hediffs;
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
        private Dictionary<BodyPartGroupDef, List<BodyPartRecord>> ableBodyPartGroupsDict =
            new Dictionary<BodyPartGroupDef, List<BodyPartRecord>>();

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
        private BodyPartGroupDef focusBodyPartGroup;

        /// <summary>
        /// 预执行的指令（在窗口关闭后执行）
        /// </summary>
        private readonly List<ButtonUnit> todoList = new List<ButtonUnit>();

        /// <summary>
        /// 菜单
        /// </summary>
        private readonly Menus DialogMenus = new Menus();

        /// <summary>
        /// 刷新屏幕，如果为true就执行刷新
        /// </summary>
        private bool flagShouldRefresh = false;

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
            this.fixer = fixer;
            //this.doCloseButton = true;
            this.doCloseX = true;
            //this.preventCameraMotion = false;
            this.draggable = true;
            this.resizeable = true;
            //this.onlyOneOfTypeAllowed = false;
            this.closeOnClickedOutside = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
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
                        this.linker.Add(thing.def.defName);
                    }
                }
                else if (Prefs.DevMode)
                {
                    Log.Message("错误：Dialog_AbuseMenu不应当在不能连接的建筑物上绘制");
                }

                Set_AbleBodyPartGroups();
                Set_AllAbuseHediff();
                IntMenus(DialogMenus);
            }
        }

        private void Set_AllAbuseHediff()
        {
            if (!this.AllHediff.NullOrEmpty()) return;
            foreach (var maltreatDef in DefDatabase<MaltreatDef>.AllDefs)
            {
                if (maltreatDef.maltreat.enableByBuilding == null ||
                    this.linker.Contains(maltreatDef.maltreat.enableByBuilding.defName))
                {
                    this.AllHediff.Add(maltreatDef);
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
            var ablePartGroupsDict = new Dictionary<BodyPartGroupDef, List<BodyPartRecord>>();
            foreach (var bodyPart in pawn.health.hediffSet.GetNotMissingParts())
            {
                foreach (var group in bodyPart.groups)
                {
                    ablePartGroupsDict.DictListAdd(group, bodyPart);
                }
            }

            ableBodyPartGroupsDict = TortureUtility.untieNestedDict(ablePartGroupsDict);
        }

        /// <summary>
        /// 一级菜单，获得所有可选Hediff
        /// </summary>
        /// <returns>Hediff列表，如果没被选中则处于disable状态</returns>
        private List<ButtonUnit> Buttons_Hediffs()
        {
            return AllHediff.Select(Button_SetHediff).ToList();
        }

        /// <summary>
        /// 二级菜单，获得所有可选BodyPartGroup，由focusHediff定义
        /// </summary>
        /// <returns>BodyPartGroup列表，如果没被选中则处于inactive状态</returns>
        private List<ButtonUnit> Buttons_BodyPartGroups()
        {
            List<ButtonUnit> list = new List<ButtonUnit>();
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
        private List<ButtonUnit> Buttons_BodyParts()
        {
            if (this.focusHediff == null || focusBodyPartGroup == null ||
                !ableBodyPartGroupsDict.ContainsKey(focusBodyPartGroup))
                return new List<ButtonUnit>();
            else
                return ableBodyPartGroupsDict[focusBodyPartGroup].Select(Button_SetJob).ToList();
        }

        private ButtonUnit Button_SetHediff(MaltreatDef hediff)
        {
            var inactive = this.focusHediff != hediff;
            var action = new Action(delegate
            {
                if (this.focusHediff != hediff)
                    this.focusHediff = hediff;
                else
                    this.focusHediff = null;
                flagShouldRefresh = true;
            });
            var button = new ButtonUnit(action, hediff.GetLabelAction(),
                hediff.GetDescriptionAction() + "\n\n"
                                              + "CT_Offer".Translate() + hediff.maltreat.enableByBuilding.label,
                inactive: inactive);
            return button;
        }

        private ButtonUnit Button_SetBodyPartGroup(BodyPartGroupDef bodyPartGroupDef)
        {
            var inactive = this.focusBodyPartGroup != bodyPartGroupDef;
            var action = new Action(delegate
            {
                if (this.focusBodyPartGroup == bodyPartGroupDef || this.focusHediff == null)
                    this.focusBodyPartGroup = null;
                else
                    this.focusBodyPartGroup = bodyPartGroupDef;
                flagShouldRefresh = true;
                //Log.Message("" + focusBodyPartGroup);
            });

            var button = new ButtonUnit(action, bodyPartGroupDef.label, "",
                disabled: ShouldBodyPartGroupDisabled(bodyPartGroupDef), inactive: inactive);
            return button;
        }

        private bool ShouldBodyPartGroupDisabled(BodyPartGroupDef bodyPartGroupDef)
        {
            if (focusHediff != null)
            {
                var list = focusHediff.maltreat.ableBodyPartGroupDefs;
                if (!list.NullOrEmpty())
                {
                    foreach (var def in list)
                    {
                        if (def.defName == bodyPartGroupDef.defName)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        private ButtonUnit Button_SetJob(BodyPartRecord bodyPart)
        {
            var h = HediffMaker.MakeHediff(this.focusHediff, this.pawn, bodyPart);
            var actionToDo = new Action(delegate { DoAddHediffJob(h, bodyPart); });
            var buttonToDo = new ButtonUnit(actionToDo,
                bodyPart.Label + "," + this.focusHediff.GetLabelAction().Colorize(this.focusHediff.defaultLabelColor),
                this.focusHediff.GetDescriptionAction());
            var actionButton = new Action(delegate
            {
                if (this.focusHediff != null && this.focusBodyPartGroup != null)
                    todoList.Add(buttonToDo);
                flagShouldRefresh = true;
            });
            var buttonButton = new ButtonUnit(actionButton, bodyPart.Label, "");
            return buttonButton;
        }

        private ButtonUnit Button_ClearToDoList()
        {
            var action = new Action(delegate
            {
                this.todoList.Clear();
                flagShouldRefresh = true;
            });
            var button = new ButtonUnit(action, "CT_Button_ClearToDoList".Translate(),
                "CT_Button_ClearToDoListDesc".Translate());
            return button;
        }

        private ButtonUnit Button_Close()
        {
            var action = new Action(delegate
            {
                foreach (var b in this.todoList)
                {
                    b.action();
                }

                this.Close();
            });
            var button = new ButtonUnit(action,
                "CT_CloseWindowForSure".Translate(),
                "CT_CloseWindowForSureDesc".Translate());
            return button;
        }

        private List<UnitStackWithIconButtonEnd> Label_TodoList()
        {
            var labels = new Dictionary<string, UnitStackWithIconButtonEnd>();
            foreach (var button in todoList)
            {
                var key = button.label;
                if (labels.ContainsKey(key))
                    labels[key].AddStack();
                else
                {
                    var label = new UnitStackWithIconButtonEnd(DeleteXSize, button);
                    label.action = new Action(delegate
                    {
                        var buttonInfo = this.todoList.Find(info => info.label == label.labelDefault);
                        this.todoList.Remove(buttonInfo);
                        flagShouldRefresh = true;
                    });
                    labels.Add(key, label);
                }
            }

            return labels.Values.ToList();
        }

        private void DoAddHediffJob(Hediff hediff, BodyPartRecord bodyPart)
        {
            pawn.health.AddHediff(hediff, bodyPart, dinfo: new DamageInfo()); //TODO 加入DInfo，把简单的添加转换为Job
        }

        class Menus
        {
            public DialogUnit titleHediffs;
            public DialogUnit titleBodyGroup;
            public DialogUnit titleBody;
            public List<DialogUnit> titleTodoList;
            public VerticalTitleWithMenu menu_Hediffs;
            public VerticalTitleWithMenu menu_BodyPartGroups;
            public VerticalTitleWithMenu menu_BodyParts;
            public VerticalTitleWithMenu menu_TodoList;

            public List<VerticalTitleWithMenu> ToList()
            {
                return new List<VerticalTitleWithMenu>()
                    { menu_Hediffs, menu_BodyPartGroups, menu_BodyParts, menu_TodoList };
            }
        }

        private void IntMenus(Menus menus)
        {
            var buttons_Hediffs = Buttons_Hediffs().ListReform<ButtonUnit, DialogUnit>();
            var buttons_BodyPartGroups = Buttons_BodyPartGroups().ListReform<ButtonUnit, DialogUnit>();
            var buttons_BodyParts = Buttons_BodyParts().ListReform<ButtonUnit, DialogUnit>();
            var label_TodoList = Label_TodoList().ListReform<UnitStackWithIconButtonEnd, DialogUnit>();
            menus.titleHediffs = new DialogUnit("CT_UsableAbuseList".Translate(), "");
            menus.titleBodyGroup = new DialogUnit("CT_PleaseChooseBodyGroup".Translate(), "");
            menus.titleBody = new DialogUnit("CT_PleaseChooseBody".Translate(), "");
            menus.titleTodoList = new List<DialogUnit> { Button_Close(), Button_ClearToDoList() };
            menus.menu_Hediffs =
                new VerticalTitleWithMenu(menus.titleHediffs, new SimpleVerticalMenu(buttons_Hediffs));
            menus.menu_BodyPartGroups =
                new VerticalTitleWithMenu(menus.titleBodyGroup, new SimpleVerticalMenu(buttons_BodyPartGroups, 10));
            menus.menu_BodyParts =
                new VerticalTitleWithMenu(menus.titleBody, new SimpleVerticalMenu(buttons_BodyParts));
            menus.menu_TodoList =
                new VerticalTitleWithMenu(menus.titleTodoList, new SimpleVerticalMenu(label_TodoList));
        }

        private void RefreshMenus(Menus menus)
        {
            if (focusBodyPartGroup != null)
            {
                if (ShouldBodyPartGroupDisabled(focusBodyPartGroup)) //如果focusBodyPartGroup不被允许
                    focusBodyPartGroup = null;
            }

            var buttons_Hediffs = Buttons_Hediffs().ListReform<ButtonUnit, IDialogAssembly>();
            var buttons_BodyPartGroups = Buttons_BodyPartGroups().ListReform<ButtonUnit, IDialogAssembly>();
            var buttons_BodyParts = Buttons_BodyParts().ListReform<ButtonUnit, IDialogAssembly>();
            var label_TodoList = Label_TodoList().ListReform<UnitStackWithIconButtonEnd, IDialogAssembly>();
            menus.titleHediffs = new DialogUnit("CT_UsableAbuseList".Translate(), "");
            menus.titleBodyGroup = new DialogUnit("CT_PleaseChooseBodyGroup".Translate(), "");
            menus.titleBody = new DialogUnit("CT_PleaseChooseBody".Translate(), "");
            menus.titleTodoList = new List<DialogUnit> { Button_Close(), Button_ClearToDoList() };
            menus.menu_Hediffs.SubMenu.InitInfo(buttons_Hediffs);
            menus.menu_BodyPartGroups.SubMenu.InitInfo(buttons_BodyPartGroups);
            menus.menu_BodyParts.SubMenu.InitInfo(buttons_BodyParts);
            menus.menu_TodoList.SubMenu.InitInfo(label_TodoList);
            menus.menu_Hediffs.InitInfo();
            menus.menu_BodyPartGroups.InitInfo();
            menus.menu_BodyParts.InitInfo();
            menus.menu_TodoList.InitInfo();
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
            RectDivider viewRect = new RectDivider(outRect, this.GetHashCode());
            var menuRects = new List<RectDivider>();
            foreach (var menu in DialogMenus.ToList())
            {
                menu.Draw(viewRect.NewCol(menu.width));
            }
        }
    }
}