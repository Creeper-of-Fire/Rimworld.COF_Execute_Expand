using System;
using System.Collections.Generic;
using System.Linq;
using COF_Torture.Patch;
using COF_Torture.Things;
using RimWorld;
using UnityEngine;
using Verse;
using ThingCategoryDefOf = RimWorld.ThingCategoryDefOf;

namespace COF_Torture
{
    public static class TortureUtility
    {
        public const float satisfySexNeedWhenOrgasm = 0.2f;
        public const float BBQNutritionFactor = 1.4f;

        public static void Orgasm(Pawn pawn, int OrgasmTimes = 1)
        {
            if (OrgasmTimes < 1)
                OrgasmTimes = 1;
            Hediff HediffOrgasm;
            bool hasOrgasmBefore;
            HediffComp_Disappears comps1 = null;
            if (pawn.health.hediffSet.HasHediff(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Orgasm))
            {
                HediffOrgasm =
                    pawn.health.hediffSet.GetFirstHediffOfDef(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Orgasm);
                hasOrgasmBefore = true;
            }
            else
            {
                HediffOrgasm = HediffMaker.MakeHediff(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Orgasm, pawn);
                hasOrgasmBefore = false;
            }

            for (int i = 0; i < OrgasmTimes; i++)
            {
                if (SettingPatch.RimJobWorldIsActive)
                {
                    Need need = pawn.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == SettingPatch.SexNeed));
                    need.CurLevel += TortureUtility.satisfySexNeedWhenOrgasm; //因为高潮获得了性满足
                }

                if (hasOrgasmBefore)
                {
                    HediffOrgasm.Severity += 1;
                    if (comps1 == null)
                        comps1 = HediffOrgasm.TryGetComp<HediffComp_Disappears>();
                    comps1.ticksToDisappear = comps1.Props.disappearsAfterTicks.RandomInRange;
                }
                //补充高潮状态，重置消失时间
                else
                {
                    pawn.health.AddHediff(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Orgasm);
                    hasOrgasmBefore = true;
                }
            }
        }

        public static void a()
        {
            foreach (var def in ThingDefGenerator_Buildings.ImpliedBlueprintAndFrameDefs()
                         .Concat(ThingDefGenerator_Meat.ImpliedMeatDefs())
                         .Concat(ThingDefGenerator_Techprints.ImpliedTechprintDefs())
                         .Concat(ThingDefGenerator_Corpses.ImpliedCorpseDefs()))
                DefGenerator.AddImpliedDef(def);
        }

        public static Corpse MakeCorpse_DifferentKind(this Pawn pawn, Building_Grave assignedGrave, bool inBed,
            float bedRotation)
        {
            if (pawn.holdingOwner != null)
            {
                Log.Warning(
                    "We can't make corpse because the pawn is in a ThingOwner. Remove him from the container first. This should have been already handled before calling this method. holder=" +
                    (object)pawn.ParentHolder);
                return (Corpse)null;
            }

            var cD = pawn.RaceProps.corpseDef;
            SetCorpseDef(ref cD, pawn.def);

            Corpse corpse = (Corpse)ThingMaker.MakeThing(cD);
            corpse.InnerPawn = pawn;
            if (corpse is BarbecueCorpse cookedCorpse)
            {
                cookedCorpse.LastPawn = pawn;
            }
            if (assignedGrave != null)
                corpse.InnerPawn.ownership.ClaimGrave(assignedGrave);
            if (inBed)
                corpse.InnerPawn.Drawer.renderer.wiggler.SetToCustomRotation(bedRotation + 180f);
            return corpse;
        }

        public static void SetCorpseDef(ref ThingDef corpseDef, ThingDef pawnDef)
        {
            corpseDef.thingClass = typeof(BarbecueCorpse);
            corpseDef.SetStatBaseValue(StatDefOf.Beauty, 5.0f); //TODO 根据pawn的美丽程度修正美观程度
            corpseDef.SetStatBaseValue(StatDefOf.FoodPoisonChanceFixedHuman, 0.0f);
            corpseDef.SetStatBaseValue(StatDefOf.Nutrition, 5.2f * BBQNutritionFactor);
            corpseDef.defName = "CT_BarbecueCorpse_" + pawnDef.defName;
            corpseDef.label = (string)"BarbecueCorpseLabel".Translate((NamedArgument)pawnDef.label);
            corpseDef.description = (string)"BarbecueCorpseDesc".Translate((NamedArgument)pawnDef.label);
            //corpseDef.thingCategories.Remove(ThingCategoryDefOf.CorpsesHumanlike);
            //corpseDef.thingCategories.Add(Things.ThingCategoryDefOf.BarbecueCorpsesHumanlike);
            corpseDef.comps.Clear();
            corpseDef.comps.Add((CompProperties)new CompProperties_Forbiddable());
            corpseDef.ingestible = new IngestibleProperties();
            corpseDef.tickerType = TickerType.Rare;
            corpseDef.ingestible.parent = pawnDef;
            IngestibleProperties ingestible = corpseDef.ingestible;
            ingestible.foodType = FoodTypeFlags.Meal;
            ingestible.sourceDef = pawnDef;
            if (pawnDef.race.IsFlesh)
                ingestible.preferability = FoodPreferability.MealFine; //TODO 根据pawn的美丽程度修正味道
            else
                ingestible.preferability = FoodPreferability.NeverForNutrition;
            ingestible.maxNumToIngestAtOnce = 1;
            ingestible.ingestEffect = EffecterDefOf.EatMeat;
            ingestible.ingestSound = SoundDefOf.RawMeat_Eat;
            ingestible.tasteThought = ThoughtDefOf.AteFineMeal; // ThoughtDefOf.AteHumanlikeMeatAsIngredient;
            ingestible.specialThoughtDirect = null; //pawnDef.race.FleshType.ateDirect;
            ingestible.specialThoughtAsIngredient = null;
            ingestible.ateEvent = HistoryEventDefOf.AteHumanMeatAsIngredient;
            
            //ingestible.foodType = FoodTypeFlags.Corpse;
            //ingestible.sourceDef = thingDef1;
            //ingestible.preferability = thingDef1.race.IsFlesh ? FoodPreferability.DesperateOnly : FoodPreferability.NeverForNutrition;
            //DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef((object) ingestible, "tasteThought", ThoughtDefOf.AteCorpse.defName);
            //ingestible.maxNumToIngestAtOnce = 1;
            //ingestible.ingestEffect = EffecterDefOf.EatMeat;
            //ingestible.ingestSound = SoundDefOf.RawMeat_Eat;
            //ingestible.specialThoughtDirect = thingDef1.race.FleshType.ateDirect;
        }
        private static float CalculateMarketValue(ThingDef raceDef)
        {
            float num1 = 0.0f;
            if (raceDef.race.meatDef != null)
            {
                int num2 = Mathf.RoundToInt(raceDef.GetStatValueAbstract(StatDefOf.MeatAmount));
                num1 += (float)num2 * raceDef.race.meatDef.GetStatValueAbstract(StatDefOf.MarketValue);
            }

            if (raceDef.race.leatherDef != null)
            {
                int num3 = Mathf.RoundToInt(raceDef.GetStatValueAbstract(StatDefOf.LeatherAmount));
                num1 += (float)num3 * raceDef.race.leatherDef.GetStatValueAbstract(StatDefOf.MarketValue);
            }

            if (raceDef.butcherProducts != null)
            {
                for (int index = 0; index < raceDef.butcherProducts.Count; ++index)
                    num1 += raceDef.butcherProducts[index].thingDef.BaseMarketValue *
                            (float)raceDef.butcherProducts[index].count;
            }

            return num1 * 0.6f;
        }
    }
}