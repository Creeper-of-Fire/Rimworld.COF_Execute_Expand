using System;
using System.Collections.Generic;
using System.Linq;
using COF_Torture.Dialog.Units;
using COF_Torture.Utility;
using UnityEngine;
using Verse;

namespace COF_Torture.Dialog.Menus
{
    public abstract class VerticalMenu : IDialogAssembly
    {
        public float width { get; protected set; }
        public float height { get; protected set; }
        protected Vector2 scrollPosition;
        public bool isScroll;
        public bool drawScroll = true;
        protected RectAggregator rectAggregator;
        public abstract void Draw(Rect outRect);
        public abstract void CalcSize();
        public abstract void Refresh();
    }

    public class SimpleVerticalMenu : VerticalMenu
    {
        private float MaxUnitWidth;
        private float MaxUnitHeight;
        private List<DialogUnit> Units;
        private List<List<DialogUnit>> Unit_SubMenus;
        private const float marginCol = 8f;
        private const float marginRow = 4f;
        private static readonly Vector2 margin = new Vector2(marginCol, marginRow);
        private int maxSubMenuRow;

        /// <summary>
        /// 获得行数最多的一列的列数
        /// </summary>
        private int maxSubMenuRows =>
            Unit_SubMenus.Aggregate(0, (current, menu) => Mathf.Max(current, menu.Count));

        /// <summary>
        /// 绘制自身
        /// </summary>
        public override void Draw(Rect inRect)
        {
            var Rect = new Rect(inRect);
            var rect = new RectDivider(Rect, Rect.GetHashCode(), margin);
            var rectRect = rect.Rect;
            if (drawScroll)
            {
                rectRect.width -= Window.StandardMargin;
            }

            var Width = Mathf.Max(width, rectRect.width);
            var UnitWidth = Mathf.Max(MaxUnitWidth, rectRect.width / Unit_SubMenus.Count - marginCol);
            if (!isScroll)
            {
                DrawCont(rect);
                return;
            }

            var viewRect = new RectDivider(new Rect(0.0f, 0.0f, Width, height), GetHashCode(), margin);

            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect, drawScroll);
            DrawCont(viewRect);
            Widgets.EndScrollView();

            void DrawCont(RectDivider contRect)
            {
                var subMenuRect = new List<RectDivider>();
                for (var index = 0; index < Unit_SubMenus.Count; index++)
                    subMenuRect.Add(contRect.NewCol(UnitWidth));
                for (var index = 0; index < Unit_SubMenus.Count; index++)
                {
                    var Col = Unit_SubMenus[index];
                    var singleRect = subMenuRect[index];
                    foreach (var unit in Col)
                    {
                        unit.Draw(singleRect.NewRow(MaxUnitHeight));
                    }
                }
            }
        }

        public override void CalcSize() //计算整个菜单需要的Rect大小，在此之前，需要获得单个单元的尺寸
        {
            CalcUnitSize();
            rectAggregator = new RectAggregator(Rect.zero, GetHashCode(), margin);
            //rectAggregator.NewCol(marginCol);
            for (var index = 0; index < Unit_SubMenus.Count; index++)
                rectAggregator.NewCol(MaxUnitWidth);
            for (var index = 0; index < maxSubMenuRows; index++)
                rectAggregator.NewRow(MaxUnitHeight);
            if (drawScroll)
                rectAggregator.NewCol(Window.StandardMargin);
            width = rectAggregator.Rect.width;
            height = rectAggregator.Rect.height;
        }

        /// <summary>
        /// 获得单个单元的尺寸
        /// </summary>
        private void CalcUnitSize()
        {
            MaxUnitHeight = 0;
            MaxUnitWidth = 0;
            foreach (var unit in Unit_SubMenus.SelectMany(Unit_SubMenu => Unit_SubMenu))
            {
                unit.CalcSize();
                MaxUnitWidth = Mathf.Max(MaxUnitWidth, unit.width);
                MaxUnitHeight = Mathf.Max(MaxUnitHeight, unit.height);
            }
        }

        /// <summary>
        /// 如果需要，把单个菜单栏变成多个菜单栏
        /// </summary>
        private void MakeSubMenuMulti(List<DialogUnit> Unit_SubMenuSingle)
        {
            Unit_SubMenus = new List<List<DialogUnit>>();
            if (maxSubMenuRow <= 0 || Unit_SubMenuSingle.Count <= maxSubMenuRow)
            {
                Unit_SubMenus.Add(Unit_SubMenuSingle);
                return;
            }

            var MenuCount = (int)Math.Ceiling((double)Unit_SubMenuSingle.Count / maxSubMenuRow);
            var aCols = 1 + Unit_SubMenuSingle.Count / MenuCount; //一竖有多少个
            for (var i = 0; i < Unit_SubMenuSingle.Count; i += aCols)
                Unit_SubMenus.Add(Unit_SubMenuSingle.Skip(i).Take(aCols).ToList());
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="units">所有的unit</param>
        /// <param name="maxSubMenuRow">每列最大行数，如果大于0则启用自动换行</param>
        /// <param name="isScroll">是否显示滚动条</param>
        /// <param name="drawScroll">是否绘制滚动条</param>
        public SimpleVerticalMenu(List<DialogUnit> units, int maxSubMenuRow = -1, bool isScroll = true,
            bool drawScroll = false)
        {
            this.maxSubMenuRow = maxSubMenuRow;
            this.isScroll = isScroll;
            this.drawScroll = drawScroll;
            Units = units;
            Refresh();
        }

        public sealed override void Refresh()
        {
            MakeSubMenuMulti(Units);
            CalcSize();
        }
    }
}