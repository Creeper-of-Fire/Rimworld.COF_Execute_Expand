using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace COF_Torture.Dialog
{
    public interface IDialogAssembly
    {
        /// <summary>
        /// 绘制自己
        /// </summary>
        void Draw(Rect rect);

        float width { get; }
        float height { get; }
    }

    public class UnitStackWithIconButtonEnd : UnitStack
    {
        public Texture2D ButtonIcon;
        public Action action;
        private int IconSize;
        private const float marginCol = 8f;
        private const float marginRow = 4f;
        private static readonly Vector2 margin = new Vector2(marginCol, marginRow);

        public override void Draw(Rect rect)
        {
            var outRect = new RectDivider(rect, this.GetHashCode(), margin);
            var rectDelButton = outRect.NewCol(IconSize, HorizontalJustification.Right);
            //var rectLabel = outRect.NewRow(height);
            Widgets.Label(outRect, this.label);
            Widgets.DrawHighlightIfMouseover(outRect);
            var rectButton = rectDelButton.NewCol(IconSize).NewRow(IconSize);
            GUI.color = Color.red;
            if (Widgets.ButtonImage(rectButton, ButtonIcon))
            {
                /*var buttonInfo = this.todoList.Find(info => info.label == stackLabel.Unit.label);
                this.todoList.Remove(buttonInfo);*/
                this.action();
            }

            GUI.color = Color.white;
            TooltipHandler.TipRegion(rectButton, "Delete".Translate());
        }

        public UnitStackWithIconButtonEnd(Action action, int IconSize, string label, string desc,
            Texture2D buttonIcon = null) : base(label, desc)
        {
            this.action = action;
            this.IconSize = IconSize;
            if (buttonIcon == null)
                ButtonIcon = TexButton.DeleteX;
            else
                ButtonIcon = buttonIcon;
        }

        public UnitStackWithIconButtonEnd(Action action, int IconSize, DialogUnit unit,
            Texture2D buttonIcon = null) : base(unit)
        {
            this.action = action;
            this.IconSize = IconSize;
            if (buttonIcon == null)
                ButtonIcon = TexButton.DeleteX;
            else
                ButtonIcon = buttonIcon;
        }

        public UnitStackWithIconButtonEnd(int IconSize, ButtonUnit unit,
            Texture2D buttonIcon = null) : base(unit)
        {
            this.action = unit.action;
            this.IconSize = IconSize;
            if (buttonIcon == null)
                ButtonIcon = TexButton.DeleteX;
            else
                ButtonIcon = buttonIcon;
        }

        public override void CalcSize()
        {
            Text.Font = font;
            var size = Text.CalcSize(this.label);
            height = Mathf.Max(size.y + LittleBlankSize, IconSize);
            width = size.x + LittleBlankSize + IconSize + marginCol;
        }
    }

    public class DialogUnit : IDialogAssembly
    {
        protected const int LittleBlankSize = 6;
        public string labelDefault { get; private set; }
        public string descDefault { get; private set; }
        public float width { get; protected set; }
        public float height { get; protected set; }
        public GameFont font { get; set; }
        public virtual string label => labelDefault;
        public virtual string desc => descDefault;

        private void InitInfo(string _label, string _desc, GameFont _font)
        {
            this.labelDefault = _label;
            this.descDefault = _desc;
            this.font = _font;
            if (this.descDefault == "No description.")
            {
                this.descDefault = "";
            }

            CalcSize();
        }

        /// <summary>
        /// 输入参数来定义的按钮
        /// </summary>
        /// <param name="label">文本</param>
        /// <param name="desc">悬浮描述</param>
        /// <param name="font">字体（默认为小）</param>
        public DialogUnit(string label, string desc, GameFont font = GameFont.Small) =>
            InitInfo(label, desc, font);

        /// <summary>
        /// 在当前矩形绘制，Rect请在外部定义
        /// </summary>
        /// <param name="rect">要绘制到的矩形范围</param>
        public virtual void Draw(Rect rect)
        {
            Text.Font = font;
            Widgets.Label(rect, label);
            TooltipHandler.TipRegion(rect, desc);
        }

        public virtual void CalcSize()
        {
            Text.Font = font;
            var size = Text.CalcSize(this.label);
            height = size.y + LittleBlankSize;
            width = size.x + LittleBlankSize;
        }
    }

    /// <summary>
    /// 多个按钮折叠起来
    /// </summary>
    public class UnitStack : DialogUnit
    {
        private int count = 1;

        public UnitStack(DialogUnit unit) : base(unit.label, unit.desc)
        {
        }

        public UnitStack(string label, string desc) : base(label, desc)
        {
        }

        public void AddStack() => count++;

        // ReSharper disable once MemberCanBePrivate.Global
        public int GetStack() => count;
        public override string label => this.labelDefault + " x" + GetStack();
    }
}