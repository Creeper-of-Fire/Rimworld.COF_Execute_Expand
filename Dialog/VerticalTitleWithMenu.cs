using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace COF_Torture.Dialog
{
    public class VerticalTitleWithMenu : VerticalMenu
    {
        public readonly List<DialogUnit> Unit_Titles;
        public readonly VerticalMenu SubMenu;
        private float MaxUnitHeight;
        private float MaxUnitWidth;

        public override void Draw(Rect inRect)
        {
            var outRect = new Rect(inRect);
            var rect = new RectDivider(outRect, this.GetHashCode());
            Widgets.DrawLineVertical(outRect.xMin - Window.StandardMargin / 2f,
                outRect.y, outRect.height);
            foreach (var unit in Unit_Titles)
            {
                unit.Draw(rect.NewRow(MaxUnitHeight));
            }

            SubMenu.Draw(rect.NewRow(rect.Rect.height));
            Widgets.DrawLineVertical(outRect.xMax + Window.StandardMargin / 2f,
                outRect.y, outRect.height);
        }

        public override void CalcSize()
        {
            CalcUnitSize();
            var Width = Mathf.Max(MaxUnitWidth, SubMenu.width);
            rectAggregator = new RectAggregator(Rect.zero, this.GetHashCode());
            rectAggregator.NewCol(Width);
            for (var index = 0; index < Unit_Titles.Count; index++)
                rectAggregator.NewRow(MaxUnitHeight);
            this.width = rectAggregator.Rect.width;
            this.height = rectAggregator.Rect.height;
        }

        /// <summary>
        /// 构造一个有标题栏和下属菜单的东西
        /// </summary>
        /// <param name="titleUnits">多个标题</param>
        /// <param name="subMenu">下属菜单</param>
        public VerticalTitleWithMenu(List<DialogUnit> titleUnits, SimpleVerticalMenu subMenu)
        {
            this.SubMenu = subMenu;
            this.Unit_Titles = titleUnits;
            InitInfo();
        }

        /// <summary>
        /// 构造一个有标题栏和下属菜单的东西
        /// </summary>
        /// <param name="titleUnit">标题</param>
        /// <param name="subMenu">下属菜单</param>
        public VerticalTitleWithMenu(DialogUnit titleUnit, SimpleVerticalMenu subMenu) :
            this(new List<DialogUnit>() { titleUnit }, subMenu)
        {
        }
        
        public sealed override void InitInfo(List<IDialogAssembly>units = null)
        {
            CalcSize();
        }

        /// <summary>
        /// 获得单个单元的尺寸
        /// </summary>
        private void CalcUnitSize()
        {
            Text.Font = GameFont.Medium;
            foreach (var unit in Unit_Titles)
            {
                MaxUnitWidth = Mathf.Max(MaxUnitWidth, unit.width);
                MaxUnitHeight = Mathf.Max(MaxUnitHeight, unit.height);
            }
        }
    }
}