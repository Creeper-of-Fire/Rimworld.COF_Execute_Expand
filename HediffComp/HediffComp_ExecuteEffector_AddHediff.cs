using System.Collections.Generic;
using COF_Torture.Hediffs;
using COF_Torture.Patch;
using COF_Torture.Things;
using Verse;
using DamageDefOf = COF_Torture.Utility.DefOf.DamageDefOf;

namespace COF_Torture.HediffComp
{
    
    /// <summary>
    /// 普通的处刑效果，不断添加Hediff
    /// </summary>
    public class HediffCompProperties_ExecuteEffector_AddHediff : HediffCompProperties
    {
        public int ticksToAdd = 100;
        public FloatRange severityToAdd;
        public int addHediffNumMax = 20;
        public int addHediffNumInt = 20;
        public HediffDef addHediff;
        public List<BodyPartDef> addBodyParts;

        public HediffCompProperties_ExecuteEffector_AddHediff() =>
            compClass = typeof(HediffComp_ExecuteEffector_AddHediff);
    }

    public class HediffComp_ExecuteEffector_AddHediff : HediffComp_ExecuteEffector
    {
        public HediffCompProperties_ExecuteEffector_AddHediff Props =>
            (HediffCompProperties_ExecuteEffector_AddHediff)props;

        public Hediff_WithGiver Parent => (Hediff_WithGiver)parent;
        public int ticksToAdd;
        public Thing giver;

        public static DamageInfo dInfo()
        {
            if (SettingPatch.RimJobWorldIsActive)
            {
                var execute = DamageDefOf.Execute_Licentious;
                return new DamageInfo(execute, 1);
            }
            else
            {
                var execute = DamageDefOf.Execute;
                return new DamageInfo(execute, 1);
            }
        }

        public virtual IEnumerable<BodyPartRecord> ListOfPart()
        {
            var partsHave = Pawn.health.hediffSet.GetNotMissingParts();
            if (partsHave == null)
            {
                yield break;
            }

            var partsAdd = Props.addBodyParts;
            //var h = (Hediff_Injury)HediffMaker.MakeHediff(Props.addHediff, Pawn);
            if (partsAdd == null)
            {
                foreach (var p in partsHave)
                {
                    yield return p;
                }

                yield break;
            }

            foreach (var p in partsHave)
            {
                if (partsAdd.Contains(p.def))
                    yield return p;
            }
        }

        /// <summary>
        /// 通过递归的方式试图来添加Hediff，递归深度限制为10
        /// </summary>
        /// <param name="depth">递归用，限制递归深度，不需要填写</param>
        public virtual void TryAddHediff(int depth = 0)
        {
            depth++;
            if (depth >= 10)
                return;
            if (giver == null)
            {
                giver = Parent.Giver;
            }

            if (AddHediff())
                TryAddHediff(depth);
        }

        /// <summary>
        /// 添加Hediff
        /// </summary>
        /// <returns>是否继续递归</returns>
        public virtual bool AddHediff()
        {
            BodyPartRecord part = ListOfPart().RandomElement();
            if (part == null)
                return false;
            HediffDef hDef = Props.addHediff;
            //Log.Message(part + "");
            Hediff_ExecuteInjury h = (Hediff_ExecuteInjury)HediffMaker.MakeHediff(hDef, Pawn, part);
            h.Severity = Props.severityToAdd.RandomInRange;
            if (Pawn.health.hediffSet.GetHediffCount(hDef) < Props.addHediffNumMax)
            {
                if (isAddAble(part, h))
                {
                    h.Giver = giver;
                    Pawn.health.AddHediff(h, part, dInfo());
                }

                return true;
            }

            return false;
        }

        private bool isAddAble(BodyPartRecord part, Hediff_Injury h)
        {
            if (part == null)
                return false;
            if (!(part.coverageAbs > 0.0))
            {
                return false;
            }

            if (Parent.Giver is Building_TortureBed bT && bT.isSafe)
            {
                if (Pawn.health.hediffSet.GetPartHealth(part) < (double)h.Severity)
                {
                    return false;
                }
            }

            return true;
        }

        protected override void ProcessTick()
        {
            if (!isInProgress) return;
            ticksToAdd++;
            if (ticksToAdd >= Props.ticksToAdd)
            {
                ticksToAdd = 0;
                TryAddHediff();
            }
        }

        public override void startExecuteProcess()
        {
            postExecuteStart();
            isInProgress = true;
        }

        public virtual void postExecuteStart()
        {
            for (int i = 0; i < Props.addHediffNumInt; i++)
            {
                TryAddHediff();
            }
        }
    }
}