using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace COF_Torture.Dialog
{
    public static class DialogUtility
    {
        public static IEnumerable<IWithGiver> AllTortureHediff(this Pawn pawn)
        {
            foreach (var h in pawn.health.hediffSet.hediffs)
            {
                if (h is IWithGiver hg && hg.Giver != null)
                {
                    yield return hg;
                }
            }
        }

        public class DoubleKeyDictionary<Key1, Key2, Value>
        {
            public Dictionary<Key1, Dictionary<Key2, Value>> multiDict =
                new Dictionary<Key1, Dictionary<Key2, Value>>();

            public void SetSet(Key1 key1, Key2 key2, Value value)
            {
                if (multiDict.ContainsKey(key1))
                {
                    var dict2 = multiDict[key1];
                    if (dict2.ContainsKey(key2))
                        dict2[key2] = value;
                    else
                        dict2.Add(key2, value);
                }
                else
                {
                    var dict2 = new Dictionary<Key2, Value>();
                    dict2.Add(key2, value);
                    multiDict.Add(key1, dict2);
                }
            }

            public void SetAdd(Key1 key1, Key2 key2, Value value)
            {
                if (multiDict.ContainsKey(key1))
                {
                    var dict2 = multiDict[key1];
                    dict2.Add(key2, value);
                }
                else
                {
                    var dict2 = new Dictionary<Key2, Value>();
                    dict2.Add(key2, value);
                    multiDict.Add(key1, dict2);
                }
            }

            public void AddAdd(Key1 key1, Key2 key2, Value value)
            {
                var dict2 = new Dictionary<Key2, Value>();
                dict2.Add(key2, value);
                multiDict.Add(key1, dict2);
            }

            public Value Get(Key1 key1, Key2 key2)
            {
                Value value = default;
                if (key1 != null)
                    if (key2 != null)
                        value = multiDict[key1][key2];
                return value;
            }

            public int Count => multiDict.Count;
        }

        public class ButtonInfo
        {
            public Action action;
            public string label;
            public bool disabled = false;
            public bool inactive = false;
            public string desc;
            public string disabledReason;
            public string inactiveReason;

            private void InitButtonInfo(Action _action, string _label, string _desc, bool _disabled,
                string _disabledReason, bool _inactive, string _inactiveReason)
            {
                this.action = _action;
                this.disabled = _disabled;
                this.label = _label;
                this.desc = _desc;
                this.disabledReason = _disabledReason;
                this.inactive = _inactive;
                this.inactiveReason = _inactiveReason;
                if (this.desc == "No description.")
                {
                    this.desc = "";
                }
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
            /// <param name="inactived">按钮是否活跃</param>
            /// <param name="inactiveReason">不活跃时的描述</param>
            public ButtonInfo(Action action, string label, string desc, bool disabled = false,
                string disabledReason = "", bool inactived = false, string inactiveReason = "") =>
                InitButtonInfo(action, label, desc, disabled, disabledReason, inactived, inactiveReason);

            public ButtonInfo(Command_Action command) =>
                InitButtonInfo(command.action, command.Label, command.Desc, command.disabled,
                    command.disabledReason, false, "");

            public ButtonInfo(Command_Toggle command) =>
                InitButtonInfo(command.toggleAction, command.Label, command.Desc, command.disabled,
                    command.disabledReason, false, "");

            /// <summary>
            /// 在当前行绘制按钮，使用NewCol
            /// </summary>
            /// <param name="rect">要绘制到的矩形范围</param>
            /// <param name="playSound">播放什么音乐</param>
            /// <param name="gameFont">字体大小（默认为小）</param>
            public void Draw(RectDivider rect,
                SoundDef playSound = null, GameFont gameFont = GameFont.Small)
            {
                Text.Font = gameFont;
                string ButtonTipText = this.desc;
                if (this.disabled)
                {
                    GUI.color = Color.grey;
                    if (!this.disabledReason.NullOrEmpty())
                        ButtonTipText = this.disabledReason;
                }

                if (this.inactive)
                {
                    GUI.color = Color.grey;
                    if (!this.inactiveReason.NullOrEmpty())
                        ButtonTipText = this.inactiveReason;
                }

                if (Widgets.ButtonText(rect, this.label) && !this.disabled)
                {
                    this.action();
                    if (playSound == null)
                        playSound = SoundDefOf.Click;
                    playSound.PlayOneShotOnCamera();
                }

                if (Mouse.IsOver(rect) && !ButtonTipText.NullOrEmpty())
                {
                    TooltipHandler.TipRegion(rect, ButtonTipText);
                }

                GUI.color = Color.white;
            }
        }

        /// <summary>
        /// 多个按钮折叠起来
        /// </summary>
        public class ButtonStack
        {
            private readonly ButtonInfo ButtonInfo;
            private int count = 1;
            public ButtonStack(ButtonInfo button) => ButtonInfo = button;
            public void AddStack() => count++;

            // ReSharper disable once MemberCanBePrivate.Global
            public int GetStack() => count;
            public ButtonInfo Button => ButtonInfo;
            public string Label => ButtonInfo.label + " x" + GetStack();
            public string LabelSingle => ButtonInfo.label;
        }

        /// <summary>
        /// 多个按钮堆叠形成的列表
        /// </summary>
        public class ButtonStacks
        {
            private readonly Dictionary<string, ButtonStack> buttonStacks = new Dictionary<string, ButtonStack>();
            private int Count => buttonStacks.Count;

            public void Add(ButtonInfo buttonInfo)
            {
                var dict = buttonStacks;
                var key = GetLabelFromButton(buttonInfo);
                if (dict.ContainsKey(key))
                    dict[key].AddStack();
                else
                    dict.Add(key, new ButtonStack(buttonInfo));
            }

            public List<ButtonStack> ToList()
            {
                return buttonStacks.Values.ToList();
            }

            private string GetLabelFromButton(ButtonInfo button)
                => button.label + button.desc;
        }

        /// <summary>
        /// 把多个Gizmo替换成多个按钮
        /// </summary>
        /// <param name="gizmos">含有Gizmo的可枚举对象</param>
        /// <returns>多个按钮</returns>
        public static IEnumerable<ButtonInfo> getButtonInfo(IEnumerable<Command> gizmos)
        {
            return gizmos.Select(getButtonInfo).Where(button => button != null);
        }

        /// <summary>
        /// 把单个Gizmo替换成单个按钮信息
        /// </summary>
        /// <param name="gizmo">Gizmo</param>
        /// <returns>按钮</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public static ButtonInfo getButtonInfo(Command gizmo)
        {
            switch (gizmo)
            {
                case Command_Action ca:
                    return new ButtonInfo(ca);
                case Command_Toggle ct:
                    return new ButtonInfo(ct);
            }

            return null;
        }

        public static void CalcWidth<T>(T menuColumn, ref float maxWidth, GameFont font = GameFont.Medium)
        {
            Text.Font = font;
            //maxHeight = Mathf.Max(maxHeight, Text.CalcHeight(GetLabel(menuColumn), width));
            maxWidth = Mathf.Max(maxWidth, Text.CalcSize(GetLabel<T>(menuColumn)).x);
            Text.Font = GameFont.Medium;
        }

        public static void CalcHeight<T>(T menuColumn, ref float maxHeight, GameFont font = GameFont.Medium)
        {
            Text.Font = font;
            maxHeight = Mathf.Max(maxHeight, Text.CalcSize(GetLabel<T>(menuColumn)).y);
            Text.Font = GameFont.Medium;
        }

        /// <summary>
        /// 获得标签（Label值），主要是用于不检查类型的对象
        /// </summary>
        /// <param name="menuColumn">无类型对象</param>
        /// <returns>Label</returns>
        public static string GetLabel<T>(T menuColumn)
        {
            if (menuColumn is Thing thingColumn)
                return GetLabel<T>(thingColumn);
            if (menuColumn is Hediff hediffColumn)
                return GetLabel<T>(hediffColumn);
            if (menuColumn is ButtonInfo buttonColumn)
                return GetLabel<T>(buttonColumn);
            if (menuColumn is string stringColumn)
                return GetLabel<T>(stringColumn);
            if (menuColumn is TaggedString tagStringColumn)
                return GetLabel<T>(tagStringColumn);
            if (menuColumn is ButtonStack buttonsColumn)
                return GetLabel<T>(buttonsColumn);
            Log.Error("[COF_Torture]" + menuColumn + "中没有发现对应的Label");
            return "";
        }

        public static string GetLabel<T>(Thing menuColumn) => menuColumn.Label;
        public static string GetLabel<T>(Hediff menuColumn) => menuColumn.Label;
        public static string GetLabel<T>(ButtonInfo menuColumn) => menuColumn.label;
        public static string GetLabel<T>(string menuColumn) => menuColumn;
        public static string GetLabel<T>(TaggedString menuColumn) => menuColumn.ToString();
        public static string GetLabel<T>(ButtonStack menuColumn) => menuColumn.Label;
    }
}