using System;
using System.Collections.Generic;
using System.Linq;
using COF_Torture.Body;
using COF_Torture.Data;
using COF_Torture.Dialog.Menus;
using COF_Torture.Dialog.Units;
using COF_Torture.Hediffs;
using COF_Torture.Utility;
using RimWorld;
using UnityEngine;
using Verse;

namespace COF_Torture.Dialog
{
    public class Dialog_ChoosePawn
    {
        public Action<Pawn> action;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action">需要对Pawn执行的动作</param>
        public Dialog_ChoosePawn(Action<Pawn> action)
        {
            this.action = action;
        }
    }
}