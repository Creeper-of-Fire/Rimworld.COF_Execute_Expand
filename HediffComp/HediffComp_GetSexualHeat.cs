using COF_Torture.Hediffs;
using Verse;

namespace COF_Torture.HediffComp
{
    public abstract class HediffComp_GetSexualHeat : Verse.HediffComp //可以获得性兴奋的HediffComp
    {
        private const int TicksToCheck = 1000;
        private Hediff_COF_Sexual_Gratification HediffOrgasmIndicator;
        private float SexualHeatGetPerHour;
        protected virtual int TicksInterval => 360;
        private const int TicksOfAnHour = 2500;

        /// <summary>
        /// 改这个就可以直接改快感条，但是希望不要频繁使用set，因为每次都会新建一个对象，导致性能不好
        /// </summary>
        private float OrgasmIndicator
        {
            get
            {
                if (HediffOrgasmIndicator == null)
                    HediffOrgasmIndicator = Hediff_COF_Sexual_Gratification.GetHediffOrgasmIndicator(this.Pawn);
                return HediffOrgasmIndicator.TotalGratification;
            }
            set
            {
                if (HediffOrgasmIndicator == null)
                    HediffOrgasmIndicator = Hediff_COF_Sexual_Gratification.GetHediffOrgasmIndicator(this.Pawn);
                HediffOrgasmIndicator.Set_Gratification(new Sexual_Gratification(value));
            }
        }

        public sealed override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn.IsHashIntervalTick(TicksToCheck))
                RefreshSexualHeatMaker();
            if (Pawn.IsHashIntervalTick(TicksInterval))
            {
                AddSexualHeat();
                ProcessTick();
            }
        }

        public void RefreshSexualHeatMaker()
        {
            SexualHeatGetPerHour = GetSexualHeatPerHour();
        }

        public void AddSexualHeat()
        {
            OrgasmIndicator += CalcSexualHeat(SexualHeatGetPerHour);
        }

        /// <summary>
        /// 把每小时获取量代换为每次Tick时的获取量
        /// </summary>
        /// <param name="severity">每小时获取量</param>
        /// <returns>每次Tick时的获取量</returns>
        protected float CalcSexualHeat(float severity)
        {
            severity /= TicksOfAnHour;
            severity *= TicksInterval;
            return severity;
        }

        /// <summary>
        /// 更新每小时的获取量
        /// </summary>
        /// <returns></returns>
        protected abstract float GetSexualHeatPerHour();

        /// <summary>
        /// 这个留空，子类自行填写
        /// </summary>
        private void ProcessTick()
        {
        }
    }

    public class HediffComp_GetSexualHeatNormal : HediffComp_GetSexualHeat //正常获得性兴奋，固定获得
    {
        private HediffCompProperties_GetSexualHeatNormal Props =>
            (HediffCompProperties_GetSexualHeatNormal)props;

        protected override float GetSexualHeatPerHour()
        {
            var props1 = Props;
            return props1.SexualHeatGetPerHour;
        }
    }

    public class HediffCompProperties_GetSexualHeatNormal : HediffCompProperties
    {
        public float SexualHeatGetPerHour = 0f;

        public HediffCompProperties_GetSexualHeatNormal() =>
            compClass = typeof(HediffComp_GetSexualHeatNormal);
    }

    public class HediffComp_GetSexualHeatBySeverity : HediffComp_GetSexualHeat //正常获得性兴奋，但是通过这个Hediff严重度和一个比例
    {
        private HediffCompProperties_GetSexualHeatBySeverity Props =>
            (HediffCompProperties_GetSexualHeatBySeverity)props;

        protected override float GetSexualHeatPerHour()
        {
            var props1 = Props;
            return parent.Severity * props1.SexualHeatConversionRatePerHour;
        }
    }

    public class HediffCompProperties_GetSexualHeatBySeverity : HediffCompProperties
    {
        public float SexualHeatConversionRatePerHour = 0f;

        public HediffCompProperties_GetSexualHeatBySeverity() =>
            compClass = typeof(HediffComp_GetSexualHeatBySeverity);
    }

    public class HediffComp_GetSexualHeatWithPain : HediffComp_GetSexualHeatNormal //通过疼痛获得性兴奋
    {
        private HediffCompProperties_GetSexualHeatWithPain Props =>
            (HediffCompProperties_GetSexualHeatWithPain)props;

        protected override float GetSexualHeatPerHour()
        {
            var props1 = Props;
            var pain = Pawn.health.hediffSet.PainTotal;
            return pain * props1.SexualHeatConversionRatePerHour;
        }
    }

    public class HediffCompProperties_GetSexualHeatWithPain : HediffCompProperties
    {
        public float SexualHeatConversionRatePerHour = 0f;

        public HediffCompProperties_GetSexualHeatWithPain() =>
            compClass = typeof(HediffComp_GetSexualHeatWithPain);
    }
}