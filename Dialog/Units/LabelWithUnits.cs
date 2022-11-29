using System;
using System.Collections.Generic;
using System.Linq;
using COF_Torture.Data;
using COF_Torture.Dialog.Menus;
using COF_Torture.Dialog.Units;
using COF_Torture.Hediffs;
using COF_Torture.Utility;
using RimWorld;
using UnityEngine;
using Verse;

namespace COF_Torture.Dialog.Units
{
    public sealed class LabelWithUnits : DialogUnit
    {
        private bool isUnitsSameWidth = true;
        private const float marginCol = 8f;
        private const float marginRow = 4f;
        private static readonly Vector2 margin = new Vector2(marginCol, marginRow);
        private readonly List<DialogUnit> SubUnits = new List<DialogUnit>();
        private float unitMaxWidth = 0f;
        private float unitTotalWidth = 0f;

        public override void Draw(Rect rect)
        {
            Text.Font = font;
            var outRect = new RectDivider(rect, this.GetHashCode(), margin);
            var labelRect = outRect.NewCol(unitTotalWidth, HorizontalJustification.Right);
            Widgets.Label(outRect, this.label);
            Widgets.DrawHighlightIfMouseover(outRect);
            foreach (var unit in SubUnits)
            {
                var rectUnit = labelRect.NewCol(isUnitsSameWidth ? unitMaxWidth : unit.width);
                unit.Draw(rectUnit);
            }
        }

        public void InitInfo(IEnumerable<DialogUnit> subUnits, string _label, string _desc, bool unitsSameWidth = true,
            GameFont _font = GameFont.Small)
        {
            this.SubUnits.Clear();
            this.SubUnits.InsertRange(0, subUnits);
            this.isUnitsSameWidth = unitsSameWidth;
            base.InitInfo(_label, _desc, _font);
        }

        public void InitInfo(DialogUnit subUnit, string _label, string _desc, bool unitsSameWidth = true,
            GameFont _font = GameFont.Small) =>
            this.InitInfo(new List<DialogUnit> { subUnit }, _label, _desc, unitsSameWidth, _font);

        public override void CalcSize()
        {
            Text.Font = font;
            var labelSize = Text.CalcSize(this.label);
            var MaxHeight = labelSize.y;
            var uMaxWidth = 0f;
            foreach (var unit in SubUnits)
            {
                MaxHeight = Mathf.Max(MaxHeight, unit.height);
                uMaxWidth = Mathf.Max(uMaxWidth, unit.width);
            }

            var rectAggregator = new RectAggregator(Rect.zero, this.GetHashCode(), margin);
            rectAggregator.NewRow(MaxHeight);
            //rectAggregator.NewCol(marginCol);
            foreach (var unit in SubUnits)
            {
                rectAggregator.NewCol(isUnitsSameWidth ? uMaxWidth : unit.width);
            }

            this.unitTotalWidth = (int)rectAggregator.Rect.width;
            rectAggregator.NewCol(labelSize.x);
            this.width = rectAggregator.Rect.width;
            this.height = rectAggregator.Rect.height;
            this.unitMaxWidth = uMaxWidth;
        }
    }
}