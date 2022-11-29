using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace COF_Torture.Dialog.Units
{
    public abstract class ButtonUnit : DialogUnit
    {
        private Action action;
        protected bool disabled = false;
        protected bool inactive = false;
        protected string disabledReason;
        protected string inactiveReason;
        protected SoundDef playSound { get; set; } = SoundDefOf.Click;
        public void Disable() => this.disabled = true;
        public void Enable() => this.disabled = false;

        /// <summary>
        /// 输入参数来定义的按钮
        /// </summary>
        /// <param name="_action">按钮执行的动作</param>
        /// <param name="_label">按钮的文本</param>
        /// <param name="_desc">按钮的悬浮描述</param>
        /// <param name="_disabled">按钮是否启用</param>
        /// <param name="_disabledReason">不启用时的描述</param>
        /// <param name="_inactive">按钮是否活跃</param>
        /// <param name="_inactiveReason">不活跃时的描述</param>
        /// <param name="_font">字体（默认为Small）</param>
        protected void InitInfo(Action _action, string _label, string _desc, GameFont _font, bool _disabled = false,
            string _disabledReason = "", bool _inactive = false, string _inactiveReason = "")
        {
            this.action = _action;
            this.disabled = _disabled;
            this.disabledReason = _disabledReason;
            this.inactive = _inactive;
            this.inactiveReason = _inactiveReason;
            base.InitInfo(_label, _desc, _font);
        }

        public void DoAction()
        {
            action();
        }
    }

    public sealed class ButtonTextUnit : ButtonUnit
    {
        public override void Draw(Rect rect)
        {
            Text.Font = font;
            string ButtonTipText = this.descDefault;
            var label_temp = this.labelDefault;
            if (this.disabled)
            {
                GUI.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                if (!this.disabledReason.NullOrEmpty())
                    ButtonTipText = this.disabledReason;
            }
            else if (this.inactive)
            {
                GUI.color = Color.grey;
                if (!this.inactiveReason.NullOrEmpty())
                    ButtonTipText = this.inactiveReason;
            }

            if (Widgets.ButtonText(rect, label_temp) && !this.disabled)
            {
                this.DoAction();
                playSound.PlayOneShotOnCamera();
            }

            if (!ButtonTipText.NullOrEmpty())
                TooltipHandler.TipRegion(rect, ButtonTipText);

            GUI.color = Color.white;
        }

        public override void CalcSize()
        {
            Text.Font = font;
            var size = Text.CalcSize(this.label);
            height = size.y + LittleBlankSize;
            width = size.x + LittleBlankSize;
        }

        public void InitInfo(Action _action, string _label, string _desc, bool _disabled = false,
            string _disabledReason = "", bool _inactive = false, string _inactiveReason = "",
            GameFont _font = GameFont.Small) =>
            base.InitInfo(_action, _label, _desc, _font, _disabled, _disabledReason, _inactive, _inactiveReason);

        public void InitInfo(Command_Action command) =>
            this.InitInfo(command.action, command.Label, command.Desc, command.disabled, command.disabledReason);

        public void InitInfo(Command_Toggle command) =>
            this.InitInfo(command.toggleAction, command.Label, command.Desc, command.disabled, command.disabledReason);
    }

    public sealed class ButtonIconUnit : ButtonUnit
    {
        private Texture2D ButtonIcon;
        private Vector2 IconSize;
        private Color color;

        public override void Draw(Rect rect)
        {
            var outRect = new RectDivider(rect, this.GetHashCode(), Vector2.zero);
            var rectButton = outRect.NewCol(this.width).NewRow(this.height);
            GUI.color = color;
            if (Widgets.ButtonImage(rectButton, ButtonIcon))
            {
                this.DoAction();
            }

            TooltipHandler.TipRegion(rectButton, "Delete".Translate());
            GUI.color = Color.white;
        }

        public void InitInfo(Action _action, Texture2D Icon, Color _color,
            float _Width = 20f,
            float _Height = 20f)
        {
            this.color = _color == default ? _color : Color.red;
            this.ButtonIcon = Icon.NullOrBad() ? Icon : TexButton.DeleteX;
            this.IconSize = new Vector2(_Width, _Height);
            base.InitInfo(_action, "IconButton" + Icon.GetHashCode() + "" + _action.GetHashCode(), "", default);
        }

        public override void CalcSize()
        {
            this.width = IconSize.x;
            this.height = IconSize.y;
        }
    }
}