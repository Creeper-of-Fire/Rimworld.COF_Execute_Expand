using System.Collections.Generic;
using COF_Torture.Cell;
using COF_Torture.Data;
using COF_Torture.Hediffs;
using COF_Torture.Things;
using Verse;

namespace COF_Torture.Body
{
    /// <summary>
    /// 一个标签，说明可以自动执行
    /// </summary>
    public class CompProperties_WithVirtualPart : CompProperties
    {
        public CompProperties_WithVirtualPart() => compClass = typeof(Comp_WithVirtualPart);
    }

    /// <summary>
    /// 一个标签，说明可以自动执行
    /// </summary>
    public class Comp_WithVirtualPart : ThingComp
    {
        
    }
}