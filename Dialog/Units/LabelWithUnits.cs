using System.Collections.Generic;
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
        private float unitMaxWidth;
        private float unitTotalWidth;

        public override void Draw(Rect rect)
        {
            Text.Font = font;
            var outRect = new RectDivider(rect, GetHashCode(), margin);
            var labelRect = outRect.NewCol(unitTotalWidth, HorizontalJustification.Right);
            Widgets.Label(outRect, label);
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
            SubUnits.Clear();
            SubUnits.InsertRange(0, subUnits);
            isUnitsSameWidth = unitsSameWidth;
            base.InitInfo(_label, _desc, _font);
        }

        public void InitInfo(DialogUnit subUnit, string _label, string _desc, bool unitsSameWidth = true,
            GameFont _font = GameFont.Small) =>
            InitInfo(new List<DialogUnit> { subUnit }, _label, _desc, unitsSameWidth, _font);

        public override void CalcSize()
        {
            Text.Font = font;
            var labelSize = Text.CalcSize(label);
            var MaxHeight = labelSize.y;
            var uMaxWidth = 0f;
            foreach (var unit in SubUnits)
            {
                MaxHeight = Mathf.Max(MaxHeight, unit.height);
                uMaxWidth = Mathf.Max(uMaxWidth, unit.width);
            }

            var rectAggregator = new RectAggregator(Rect.zero, GetHashCode(), margin);
            rectAggregator.NewRow(MaxHeight);
            //rectAggregator.NewCol(marginCol);
            foreach (var unit in SubUnits)
            {
                rectAggregator.NewCol(isUnitsSameWidth ? uMaxWidth : unit.width);
            }

            unitTotalWidth = (int)rectAggregator.Rect.width;
            rectAggregator.NewCol(labelSize.x);
            width = rectAggregator.Rect.width;
            height = rectAggregator.Rect.height;
            unitMaxWidth = uMaxWidth;
        }
    }
}