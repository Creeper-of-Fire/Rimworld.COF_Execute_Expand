using System.Collections.Generic;
using COF_Torture.Dialog.Menus;
using Verse;

namespace COF_Torture.Dialog
{
    public abstract class Dialog_MenuBase:Window
    {
        /// <summary>
        /// 刷新屏幕，如果为true就执行刷新
        /// </summary>
        protected bool flagShouldRefresh;
        
        /// <summary>
        /// 放入需要存储的菜单组件，而不是任其丢失
        /// </summary>
        protected abstract class MenuBase
        {
            //public List<VerticalTitleWithMenu> titleWithMenu;
        }
        
        /// <summary>
        /// 初始化菜单
        /// </summary>
        protected abstract void IntMenus();
        /// <summary>
        /// 刷新菜单
        /// </summary>
        protected abstract void RefreshMenus();
    }
}