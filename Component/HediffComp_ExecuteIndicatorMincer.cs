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
    public class HediffCompProperties_ExecuteIndicatorMincer : HediffCompProperties
    {
        public HediffCompProperties_ExecuteIndicatorMincer() => this.compClass = typeof(HediffComp_ExecuteIndicator);
    }

    public class HediffComp_ExecuteIndicatorMincer : HediffComp_ExecuteIndicator //最重要的comp，处理绝大多数的处刑相关内容——特别是关于殖民者何时死亡
    {
        public bool isButcherDone;
        private const int ticksToCount = 120;
        //private Hediff bloodLoss;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<bool>(ref isButcherDone, "isButcherDone", false);
        }

        public override void SeverityProcess()
        {
            if (isButcherDone)
            {
                if (this.Parent.Giver is Building_TortureBed bT && !bT.isSafe)
                    this.KillByExecute();
            }
        }
    }
}