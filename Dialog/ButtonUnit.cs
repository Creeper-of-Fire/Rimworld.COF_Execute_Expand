using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace COF_Torture.Dialog
{
    public class ButtonUnit : DialogUnit
    {
        public Action action;
        public bool disabled = false;
        public bool inactive = false;
        public string disabledReason;
        public string inactiveReason;
        public SoundDef playSound { get; set; } = SoundDefOf.Click;

        private void InitInfo(Action _action, bool _disabled,
            string _disabledReason, bool _inactive, string _inactiveReason)
        {
            this.action = _action;
            this.disabled = _disabled;
            this.disabledReason = _disabledReason;
            this.inactive = _inactive;
            this.inactiveReason = _inactiveReason;
        }

        public void Disable() => this.disabled = true;
        public void Enable() => this.disabled = false;

        /// <summary>
        /// 输入参数来定义的按钮
        /// </summary>
        /// <param name="action">按钮执行的动作</param>
        /// <param name="label">按钮的文本</param>
        /// <param name="desc">按钮的悬浮描述</param>
        /// <param name="disabled">按钮是否启用</param>
        /// <param name="disabledReason">不启用时的描述</param>
        /// <param name="inactive">按钮是否活跃</param>
        /// <param name="inactiveReason">不活跃时的描述</param>
        public ButtonUnit(Action action, string label, string desc, bool disabled = false,
            string disabledReason = "", bool inactive = false, string inactiveReason = "") : base(label, desc) =>
            InitInfo(action, disabled, disabledReason, inactive, inactiveReason);

        public ButtonUnit(Command_Action command) :
            this(command.action, command.Label, command.Desc, command.disabled, command.disabledReason)
        {
        }

        public ButtonUnit(Command_Toggle command) :
            this(command.toggleAction, command.Label, command.Desc, command.disabled, command.disabledReason)
        {
        }

        /// <summary>
        /// 在当前行绘制按钮，使用NewCol
        /// </summary>
        /// <param name="rect">要绘制到的矩形范围</param>
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
                this.action();
                playSound.PlayOneShotOnCamera();
            }

            if (!ButtonTipText.NullOrEmpty())
                TooltipHandler.TipRegion(rect, ButtonTipText);

            GUI.color = Color.white;
        }
    }
}