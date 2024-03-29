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
    public sealed class Dialog_ChoosePawn : Dialog_MenuBase
    {
        private Action<Pawn> pawnAction;
        private Pawn choosePawn;
        private List<Pawn> ablePawns;

        /// <summary>
        /// 菜单，用于保存数据
        /// </summary>
        private readonly Menus DialogMenus = new Menus();

        private class Menus : MenuBase
        {
            public List<DialogUnit> pawnList = new List<DialogUnit>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action">需要对Pawn执行的动作</param>
        /// <param name="map">所在的地图</param>
        /// <param name="isPawnAble">判断是否可行的函数</param>
        public Dialog_ChoosePawn(Action<Pawn> action, Map map, Func<Pawn, bool> isPawnAble)
        {
            this.pawnAction = action;
            this.ablePawns = GetAblePawn(map, isPawnAble).ToList();
            IntMenus();
            doCloseX = true;
            doCloseButton = true;
            draggable = true;
            resizeable = true;
            //closeOnClickedOutside = true;
            //absorbInputAroundWindow = true;
            forcePause = true;
        }

        private static IEnumerable<Pawn> GetAblePawn(Map map, Func<Pawn, bool> isPawnAble)
        {
            return TortureUtility.ListAllPlayerPawns(map).Where(isPawnAble);
        }

        private List<ButtonTextUnit> Buttons()
        {
            var list = new List<ButtonTextUnit>();
            foreach (var pawn in ablePawns)
            {
                list.Add(ButtonChoosePawn(pawn));
            }

            return list;
        }

        public ButtonTextUnit ButtonChoosePawn(Pawn pawn)
        {
            var button = new ButtonTextUnit();
            var action = new Action(delegate
            {
                choosePawn = pawn;
                flagShouldRefresh = true;
            });
            button.InitInfo(action, pawn.Label, pawn.DescriptionDetailed, _inactive: (choosePawn != pawn));
            return button;
        }

        public override void Close(bool doCloseSound = true)
        {
            if (!choosePawn.DestroyedOrNull())
                pawnAction(choosePawn);
            base.Close(doCloseSound);
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (flagShouldRefresh)
            {
                flagShouldRefresh = false;
                RefreshMenus();
            }

            Rect outRect = new Rect(inRect);
            outRect.yMin += StandardMargin;
            outRect.yMax -= StandardMargin * 2;
            RectDivider viewRect = new RectDivider(outRect, GetHashCode());
            foreach (var menu in DialogMenus.pawnList)
            {
                menu.Draw(viewRect.NewRow(menu.height));
            }
        }

        protected override void IntMenus()
        {
            DialogMenus.pawnList = Buttons().ListReform<ButtonTextUnit, DialogUnit>();
        }

        protected override void RefreshMenus()
        {
            DialogMenus.pawnList = Buttons().ListReform<ButtonTextUnit, DialogUnit>();
        }
    }
}