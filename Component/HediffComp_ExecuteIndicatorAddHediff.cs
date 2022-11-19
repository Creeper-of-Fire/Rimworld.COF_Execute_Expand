using System.Collections.Generic;
using COF_Torture.Hediffs;
using COF_Torture.ModSetting;
using COF_Torture.Patch;
using COF_Torture.Things;
using RimWorld;
using Verse;
using HediffDefOf = RimWorld.HediffDefOf;

namespace COF_Torture.Component
{
    public class HediffCompProperties_ExecuteIndicatorAddHediff : HediffCompProperties
    {
        public HediffCompProperties_ExecuteIndicatorAddHediff() => this.compClass = typeof(HediffComp_ExecuteIndicatorAddHediff);
    }

    public class HediffComp_ExecuteIndicatorAddHediff : HediffComp_ExecuteIndicator //最重要的comp，处理绝大多数的处刑相关内容——特别是关于殖民者何时死亡
    {
        public override void StartProgress()
        {
            var h = this.Parent.TryGetComp<HediffComp_ExecuteAddHediff>();
            if (h != null)
            {
                h.StartProcess();
                this.Parent.GiverAsInterface.startExecuteProgress();
                isInProgress = true;
            }
            else
            {
                Log.Error("[COF_Torture]类型"+this.GetType()+"未找到对应的comp");
            }
        }
    }
}