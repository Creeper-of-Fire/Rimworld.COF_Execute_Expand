using System.Collections.Generic;
using System.Linq;
using COF_Torture.Hediffs;
using COF_Torture.ModSetting;
using COF_Torture.Patch;
using COF_Torture.Things;
using RimWorld;
using UnityEngine;
using Verse;

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
            this.compClass = typeof(HediffComp_ExecuteEffector_AddHediff);
    }

    public class HediffComp_ExecuteEffector_AddHediff : HediffComp_ExecuteEffector
    {
        public HediffCompProperties_ExecuteEffector_AddHediff Props =>
            (HediffCompProperties_ExecuteEffector_AddHediff)this.props;

        public Hediff_WithGiver Parent => (Hediff_WithGiver)this.parent;
        public int ticksToAdd;
        public Thing giver;

        public static DamageInfo dInfo()
        {
            if (SettingPatch.RimJobWorldIsActive)
            {
                var execute = Damages.DamageDefOf.Execute_Licentious;
                return new DamageInfo(execute, 1);
            }
            else
            {
                var execute = Damages.DamageDefOf.Execute;
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

            var partsAdd = this.Props.addBodyParts;
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
                giver = this.Parent.Giver;
            }

            if (AddHediff())
                this.TryAddHediff(depth);
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
            HediffDef hDef = this.Props.addHediff;
            //Log.Message(part + "");
            Hediff_ExecuteInjury h = (Hediff_ExecuteInjury)HediffMaker.MakeHediff(hDef, Pawn, part);
            h.Severity = this.Props.severityToAdd.RandomInRange;
            if (Pawn.health.hediffSet.GetHediffCount(hDef) < this.Props.addHediffNumMax)
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
            if (!((double)part.coverageAbs > 0.0))
            {
                return false;
            }

            if (this.Parent.Giver is Building_TortureBed bT && bT.isSafe)
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
            if (ticksToAdd >= this.Props.ticksToAdd)
            {
                ticksToAdd = 0;
                TryAddHediff();
            }
        }

        public override void startExecuteProcess()
        {
            postExecuteStart();
            this.isInProgress = true;
        }

        public virtual void postExecuteStart()
        {
            for (int i = 0; i < this.Props.addHediffNumInt; i++)
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
            this.compClass = typeof(HediffComp_ExecuteEffector_Mincer);
    }

    public class HediffComp_ExecuteEffector_Mincer : HediffComp_ExecuteEffector_AddHediff
    {
        public new HediffCompProperties_ExecuteEffector_Mincer Props =>
            (HediffCompProperties_ExecuteEffector_Mincer)this.props;

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
                        if ((double)part.coverageAbs > 0.0)
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
            var hDef = this.Props.addHediff;
            Hediff_ExecuteInjury h = (Hediff_ExecuteInjury)HediffMaker.MakeHediff(hDef, Pawn, part);
            h.Severity = this.Props.severityToAdd.RandomInRange;
            if (part == null || !((double)part.coverageAbs > 0.0)) return true;
            h.Giver = giverOfHediff;
            Pawn.health.AddHediff(h, part, dInfo());
            productBar += h.Severity * Props.meatPerSeverity;
            return false;

        }

        protected override void ProcessTick()
        {
            ticksToAdd++;
            if (ticksToAdd < this.Props.ticksToAdd) return;
            ticksToAdd = 0;
            if (initialParts > 0)
            {
                var severity = (1 - (float)AllPartsNotBone().Count() / initialParts) *
                               this.Parent.def.lethalSeverity;
                //Log.Message(severity.ToString());
                severity = Mathf.Min(severity, this.Parent.def.lethalSeverity);
                severity = Mathf.Max(severity, 0.01f);
                this.Parent.Severity = severity;
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
                GenPlace.TryPlaceThing(thing, this.Parent.Giver.Position + (new IntVec3(0, 0, -2)),
                    this.Parent.Giver.Map, ThingPlaceMode.Near,
                    out Thing _);
            }
        }

        public override void postExecuteStart()
        {
        }
    }
}