using COF_Torture.Utility;
using UnityEngine;
using Verse;

namespace COF_Torture.Dialog.Units
{
    public abstract class DialogUnit : IDialogAssembly
    {
        protected const int LittleBlankSize = 6;
        public string labelDefault { get; private set; }
        public string descDefault { get; private set; }
        public float width { get; protected set; }
        public float height { get; protected set; }
        public GameFont font { get; set; }
        public virtual string label => labelDefault;
        public virtual string desc => descDefault;

        /// <summary>
        /// 本方法应当包含CalcSize方法
        /// </summary>
        /// <param name="_label">文本</param>
        /// <param name="_desc">悬浮描述</param>
        /// <param name="_font">字体</param>
        protected void InitInfo(string _label, string _desc, GameFont _font)
        {
            labelDefault = _label;
            descDefault = _desc;
            font = _font;
            if (descDefault == "No description.")
            {
                descDefault = "";
            }

            CalcSize();
        }

        /// <summary>
        /// 在当前矩形绘制，Rect请在外部定义
        /// </summary>
        /// <param name="rect">要绘制到的矩形范围</param>
        public abstract void Draw(Rect rect);

        /// <summary>
        /// 应当去实现这个方法
        /// </summary>
        public abstract void CalcSize();
    }
}