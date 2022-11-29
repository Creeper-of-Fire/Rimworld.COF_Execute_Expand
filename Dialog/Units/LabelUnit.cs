using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace COF_Torture.Dialog.Units
{
    public class LabelUnit : DialogUnit
    {
        /// <summary>
        /// 输入参数来定义的文本组件,本方法应当包含CalcSize方法
        /// </summary>
        /// <param name="_label">文本</param>
        /// <param name="_desc">悬浮描述</param>
        /// <param name="_font">字体（默认为小）</param>
        public new void InitInfo(string _label, string _desc, GameFont _font = GameFont.Small)
        {
            base.InitInfo(_label, _desc, _font);
        }

        /// <summary>
        /// 在当前矩形绘制，Rect请在外部定义
        /// </summary>
        /// <param name="rect">要绘制到的矩形范围</param>
        public override void Draw(Rect rect)
        {
            Text.Font = font;
            Widgets.Label(rect, label);
            TooltipHandler.TipRegion(rect, desc);
        }

        /// <summary>
        /// 应当去实现这个方法
        /// </summary>
        public override void CalcSize()
        {
            Text.Font = font;
            var size = Text.CalcSize(this.label);
            height = size.y + LittleBlankSize;
            width = size.x + LittleBlankSize;
        }
    }
    
    /// <summary>
    /// 多个文本折叠起来
    /// </summary>
    public class UnitStack : LabelUnit
    {
        private int count = 1;

        public void InitInfo(DialogUnit unit)
        {
            base.InitInfo(unit.label, unit.desc);
        }

        public void AddStack() => count++;

        // ReSharper disable once MemberCanBePrivate.Global
        public int GetStack() => count;
        public override string label => this.labelDefault + " x" + GetStack();

        public static List<UnitStack> stackList(List<DialogUnit> UnStackList)
        {
            var labels = new Dictionary<string, UnitStack>();
            foreach (var button in UnStackList)
            {
                var key = button.label;
                if (labels.ContainsKey(key))
                    labels[key].AddStack();
                else
                {
                    var label = new UnitStack();
                    label.InitInfo(button);
                    labels.Add(key, label);
                }
            }

            return labels.Values.ToList();
        }
    }
}