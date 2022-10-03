using System.Collections.Generic;
using System.Linq;
using COF_Torture.Hediffs;
using COF_Torture.ModSetting;
using COF_Torture.Patch;
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

    public class HediffComp_ExecuteMincer : HediffComp
    {
        public HediffCompProperties_ExecuteMincer Props => (HediffCompProperties_ExecuteMincer)this.props;
        public BodyPartHeight height;
        public Hediff_WithGiver Parent => (Hediff_WithGiver)this.parent;
        public int ticksToAdd;
        public Thing giverOfHediff;
        public float productBar;

        public static bool notOnlyBone(List<BodyPartRecord> partsHave)
        {
            bool notOnly = false;
            foreach (var p in partsHave)
            {
                if (p.def.destroyableByDamage)
                    if (p.def != BodyPartDefOf.Torso)
                        if ((!p.IsInGroup(BodyPartGroupDefOf.FullHead) && p.def != BodyPartDefOf.Neck) ||
                            !ModSettingMain.Instance.Setting.leftHead)
                            if ((double)p.coverageAbs > 0.0)
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

                yield break;
            }

            height = BodyPartHeight.Undefined;
            partsHave = Pawn.health.hediffSet.GetNotMissingParts(height: height).ToList();
            if (notOnlyBone(partsHave))
            {
                foreach (var p in partsHave)
                {
                    yield return p;
                }
            }
        }

        public void addHediff()
        {
            if (giverOfHediff == null)
            {
                giverOfHediff = this.Parent.giver;
            }
            var parts = ListOfPart().ToList();
            if (!parts.Any())
            {
                var comp = Parent.TryGetComp<HediffComp_ExecuteIndicator>();
                if (comp != null) comp.isButcherDone = true;
                return;
            }
            foreach (var VARIABLE in parts)
            {
                Log.Message(VARIABLE + "");
            }

            var part = parts.RandomElement();
            var hDef = this.Props.addHediff;
            Hediff_ExecuteInjury h = (Hediff_ExecuteInjury)HediffMaker.MakeHediff(hDef, Pawn, part);
            h.Severity = this.Props.severityToAdd.RandomInRange;
            if (part != null && (double)part.coverageAbs > 0.0)
            {
                h.giver = giverOfHediff;
                Pawn.health.AddHediff(h, part, dInfo());
                productBar += h.Severity*Props.meatPerSeverity;
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
            base.CompPostTick(ref severityAdjustment);
            ticksToAdd++;
            if (ticksToAdd >= this.Props.ticksToAdd)
            {
                ticksToAdd = 0;
                addHediff();
                productMeat();
            }
            else
                return;
            
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
                GenPlace.TryPlaceThing(thing, this.Parent.giver.Position+(new IntVec3(0,0,-2)), this.Parent.giver.Map, ThingPlaceMode.Near,
                    out Thing _);
            }
        }

    }
}