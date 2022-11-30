using System.Collections.Generic;
using System.Linq;
using COF_Torture.Dialog;
using COF_Torture.Hediffs;
using COF_Torture.ModSetting;
using COF_Torture.Patch;
using COF_Torture.Things;
using RimWorld;
using UnityEngine;
using Verse;
using DamageDefOf = COF_Torture.Utility.DefOf.DamageDefOf;

namespace COF_Torture.Component
{
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

    public class HediffCompProperties_ExecuteEffector_Mincer : HediffCompProperties
    {
        public int ticksToAdd = 100;
        public FloatRange severityToAdd;
        public HediffDef addHediff;
        public float meatPerSeverity;

        public HediffCompProperties_ExecuteEffector_Mincer() =>
            compClass = typeof(HediffComp_ExecuteEffector_Mincer);
    }

    public class HediffComp_ExecuteEffector_Mincer : HediffComp_ExecuteEffector_AddHediff
    {
        public new HediffCompProperties_ExecuteEffector_Mincer Props =>
            (HediffCompProperties_ExecuteEffector_Mincer)props;

        public BodyPartHeight height;
        public Thing giverOfHediff;
        public Building_TortureBed GiverOfHediff => (Building_TortureBed)giverOfHediff;
        public int initialParts;
        public float productBar;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref initialParts, "initialParts");
        }

        public static bool notBone(BodyPartRecord part)
        {
            if (part.def.destroyableByDamage)
                if (part.def != BodyPartDefOf.Torso)
                    if ((!part.IsInGroup(BodyPartGroupDefOf.FullHead) && part.def != BodyPartDefOf.Neck) ||
                        !ModSettingMain.Instance.Setting.leftHead)
                        if (part.coverageAbs > 0.0)
                            return true;
            return false;
        }

        public static bool notOnlyBone(List<BodyPartRecord> partsHave)
        {
            bool notOnly = false;
            foreach (var p in partsHave)
            {
                if (notBone(p))
                    notOnly = true;
            }

            return notOnly;
        }

        public override IEnumerable<BodyPartRecord> ListOfPart()
        {
            height = BodyPartHeight.Bottom;
            var partsHave = Pawn.health.hediffSet.GetNotMissingParts(height: height).ToList();
            if (notOnlyBone(partsHave))
            {
                foreach (var p in partsHave)
                {
                    yield return p;
                }

                yield break;
            }

            height = BodyPartHeight.Middle;
            partsHave = Pawn.health.hediffSet.GetNotMissingParts(height: height).ToList();
            if (notOnlyBone(partsHave))
            {
                foreach (var p in partsHave)
                {
                    yield return p;
                }

                yield break;
            }

            height = BodyPartHeight.Top;
            partsHave = Pawn.health.hediffSet.GetNotMissingParts(height: height).ToList();
            if (notOnlyBone(partsHave))
            {
                foreach (var p in partsHave)
                {
                    if ((!p.IsInGroup(BodyPartGroupDefOf.FullHead) && p.def != BodyPartDefOf.Neck) ||
                        !ModSettingMain.Instance.Setting.leftHead)
                        yield return p;
                }
            }
        }

        public override bool AddHediff()
        {
            var parts = ListOfPart().ToList();
            if (!parts.Any())
            {
                var comp = Parent.TryGetComp<HediffComp_ExecuteIndicatorMincer>();
                if (comp != null) comp.isButcherDone = true;
                var building = GiverOfHediff;
                if (building != null) building.showVictimBody = false;
                return false;
            }

            var part = parts.RandomElement();
            var hDef = Props.addHediff;
            Hediff_ExecuteInjury h = (Hediff_ExecuteInjury)HediffMaker.MakeHediff(hDef, Pawn, part);
            h.Severity = Props.severityToAdd.RandomInRange;
            if (part == null || !(part.coverageAbs > 0.0)) return true;
            h.Giver = giverOfHediff;
            Pawn.health.AddHediff(h, part, dInfo());
            productBar += h.Severity * Props.meatPerSeverity;
            return false;

        }

        protected override void ProcessTick()
        {
            ticksToAdd++;
            if (ticksToAdd < Props.ticksToAdd) return;
            ticksToAdd = 0;
            if (initialParts > 0)
            {
                var severity = (1 - (float)AllPartsNotBone().Count() / initialParts) *
                               Parent.def.lethalSeverity;
                //Log.Message(severity.ToString());
                severity = Mathf.Min(severity, Parent.def.lethalSeverity);
                severity = Mathf.Max(severity, 0.01f);
                Parent.Severity = severity;
            }

            TryAddHediff();
            productMeat();
        }

        public IEnumerable<BodyPartRecord> AllPartsNotBone()
        {
            var partsHave = Pawn.health.hediffSet.GetNotMissingParts().ToList();
            foreach (var p in partsHave)
            {
                if (notBone(p))
                    yield return p;
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            initialParts = AllPartsNotBone().Count();
        }

        private void productMeat()
        {
            if (productBar <= 1)
            {
                return;
            }

            if (Pawn.RaceProps.meatDef != null)
            {
                Thing thing = ThingMaker.MakeThing(Pawn.RaceProps.meatDef);
                thing.stackCount = (int)productBar;
                productBar -= (int)productBar;
                GenPlace.TryPlaceThing(thing, Parent.Giver.Position + (new IntVec3(0, 0, -2)),
                    Parent.Giver.Map, ThingPlaceMode.Near,
                    out Thing _);
            }
        }

        public override void postExecuteStart()
        {
        }
    }
}