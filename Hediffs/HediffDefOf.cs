using RimWorld;
using Verse;

namespace COF_Torture.Hediffs
{
    [DefOf]
    public static class HediffDefOf
    {
        /*private static HediffDef Torture_NeverPainDowned;
        private static HediffDef COF_Torture_NeverPainDowned
        {
            get
            {
                var def = Torture_NeverPainDowned;
                if (Torture_NeverPainDowned != null)
                {
                    //Log.Message("屹立不倒1!");
                    return def;
                }
                //Log.Message("屹立不倒2!");
                return (Torture_NeverPainDowned =
                    DefDatabase<HediffDef>.GetNamed("COF_Torture_NeverPainDowned"));
            }
        }*/
        //虽然这里会IDE报错但是实际上没有任何问题，怎么回事呢
        public static HediffDef COF_Torture_NeverPainDowned;
        public static HediffDef COF_Torture_Orgasm;
        public static HediffDef COF_Torture_SexualHeatWithPain;
        public static HediffDef COF_Torture_Fixed;
        public static HediffDef COF_Torture_OrgasmIndicator;
        public static HediffDef COF_Torture_Licentious;
        public static HediffDef COF_Torture_IsAbusing;
        public static HediffDef COF_Torture_Barbecued;
    }

}