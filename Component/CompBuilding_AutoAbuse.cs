using System.Collections.Generic;
using COF_Torture.Cell;
using COF_Torture.Data;
using COF_Torture.Hediffs;
using COF_Torture.Things;
using Verse;

namespace COF_Torture.Component
{
    /// <summary>
    /// 一个标签，说明可以自动执行
    /// </summary>
    public class CompPropertiesBuilding_AutoAbuse : CompProperties
    {
        public CompPropertiesBuilding_AutoAbuse() => compClass = typeof(CompBuilding_AutoAbuse);
    }

    /// <summary>
    /// 一个标签，说明可以自动执行
    /// </summary>
    public class CompBuilding_AutoAbuse : ThingComp
    {
        public static bool IsAuto(Building building )
        {
            if (building?.TryGetComp<CompBuilding_AutoAbuse>() == null)
                return false;
            return true;
        }
    }
}