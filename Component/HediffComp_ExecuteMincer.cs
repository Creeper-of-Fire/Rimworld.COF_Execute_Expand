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
    public class HediffCompProperties_ExecuteMincer : HediffCompProperties
    {
        public int ticksToAdd = 100;
        public FloatRange severityToAdd;
        public HediffDef addHediff;
        public float meatPerSeverity;
        public HediffCompProperties_ExecuteMincer() => this.compClass = typeof(HediffComp_ExecuteMincer);
    }

    public class HediffComp_ExecuteMincer : HediffComp //绞肉机的处刑进度，绞肉机不使用executeIndicator
    {
        public HediffCompProperties_ExecuteMincer Props => (HediffCompProperties_ExecuteMincer)this.props;
        public BodyPartHeight height;
        public Hediff_WithGiver Parent => (Hediff_WithGiver)this.parent;
        public int ticksToAdd;
        public Thing giverOfHediff;
        public Building_TortureBed GiverOfHediff => (Building_TortureBed)giverOfHediff;
        public int initialParts;

        public float productBar;
        //public List<Hediff_WithGiver> hediffList;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<int>(ref initialParts, "initialParts");
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

        public IEnumerable<BodyPartRecord> ListOfPart()
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

            //if (ModSettingMain.Instance.Setting.leftHead)
            //    yield break;

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
                //yield break;
            }

            /*height = BodyPartHeight.Undefined;
            partsHave = Pawn.health.hediffSet.GetNotMissingParts(height: height).ToList();
            if (notOnlyBone(partsHave))
            {
                foreach (var p in partsHave)
                {
                    yield return p;
                }
            }*/
        }

        public void addHediff()
        {
            if (giverOfHediff == null)
            {
                giverOfHediff = this.Parent.Giver;
            }

            var parts = ListOfPart().ToList();
            if (!parts.Any())
            {
                var comp = Parent.TryGetComp<HediffComp_ExecuteIndicatorMincer>();
                if (comp != null) comp.isButcherDone = true;
                var building = GiverOfHediff;
                if (building != null) building.showVictimBody = false;
                return;
            }
            //foreach (var VARIABLE in parts)
            //{
            //    Log.Message(VARIABLE + "");
            //}

            var part = parts.RandomElement();
            var hDef = this.Props.addHediff;
            Hediff_ExecuteInjury h = (Hediff_ExecuteInjury)HediffMaker.MakeHediff(hDef, Pawn, part);
            h.Severity = this.Props.severityToAdd.RandomInRange;
            if (part != null && (double)part.coverageAbs > 0.0)
            {
                h.Giver = giverOfHediff;
                Pawn.health.AddHediff(h, part, dInfo());
                productBar += h.Severity * Props.meatPerSeverity;
                return;
            }

            this.addHediff(); //递归，开始下一次
        }

        public static DamageInfo dInfo()
        {
            if (SettingPatch.RimJobWorldIsActive)
            {
                var execute = Damages.DamageDefOf.Execute;
                return new DamageInfo(execute, 1);
            }

            return new DamageInfo();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            //base.CompPostTick(ref severityAdjustment);
            ticksToAdd++;
            if (ticksToAdd >= this.Props.ticksToAdd)
            {
                ticksToAdd = 0;
                if (initialParts > 0)
                {
                    var severity = (1-(float)AllPartsNotBone().Count() / initialParts)*this.Parent.def.lethalSeverity;
                    Log.Message(severity.ToString());
                    severity=Mathf.Min(severity, this.Parent.def.lethalSeverity);
                    severity=Mathf.Max(severity, 0.01f);
                    this.Parent.Severity = severity;
                }
                
                addHediff();
                productMeat();
            }
            else
                return;
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

        public void productMeat()
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
    }
}