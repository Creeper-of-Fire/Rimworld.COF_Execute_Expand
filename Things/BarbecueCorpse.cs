using System.Collections.Generic;
using System.Linq;
using COF_Torture.Utility;
using RimWorld;
using UnityEngine;
using Verse;

namespace COF_Torture.Things
{
    public class BarbecueCorpse : Corpse
    {
        public Pawn LastPawn;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref LastPawn, "LastPawn");
        }

        public override void Tick()
        {
            base.Tick();
            //Log.Message(""+InnerPawn.story?.SkinColor);
            //Log.Message(""+InnerPawn.story?.SkinColorOverriden);
        }

        public override void PostMake()
        {
            base.PostMake();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            
            base.Destroy(mode);
        }

        protected override void IngestedCalculateAmounts(
            Pawn ingester,
            float nutritionWanted,
            out int numTaken,
            out float nutritionIngested)
        {
            BodyPartRecord bodyPartRecord = GetBestBodyPartToEat(ingester, nutritionWanted/TortureUtility.BBQNutritionFactor);
            if (bodyPartRecord == null)
            {
                Log.Error(ingester + " ate " + this +
                          " but no body part was found. Replacing with core part.");
                bodyPartRecord = InnerPawn.RaceProps.body.corePart;
            }

            float bodyPartNutrition = FoodUtility.GetBodyPartNutrition(this, bodyPartRecord);
            if (bodyPartRecord == InnerPawn.RaceProps.body.corePart)
            {
                if (PawnUtility.ShouldSendNotificationAbout(InnerPawn) && InnerPawn.RaceProps.Humanlike)
                    Messages.Message(
                        "MessageEatenByPredator".Translate((NamedArgument)InnerPawn.LabelShort,
                            ingester.Named("PREDATOR"), InnerPawn.Named("EATEN")).CapitalizeFirst(),
                        (Thing)ingester, MessageTypeDefOf.NegativeEvent);
                numTaken = 1;
            }
            else
            {
                Hediff_MissingPart hediffMissingPart =
                    (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, InnerPawn,
                        bodyPartRecord);
                hediffMissingPart.lastInjury = HediffDefOf.Bite;
                hediffMissingPart.IsFresh = true;
                InnerPawn.health.AddHediff(hediffMissingPart);
                numTaken = 0;
            }

            nutritionIngested = bodyPartNutrition*TortureUtility.BBQNutritionFactor;
        }
        private BodyPartRecord GetBestBodyPartToEat(Pawn ingester, float nutritionWanted)
        {
            IEnumerable<BodyPartRecord> source = InnerPawn.health.hediffSet.GetNotMissingParts().Where(x => x.depth == BodyPartDepth.Outside && FoodUtility.GetBodyPartNutrition(this, x) > 1.0 / 1000.0);
            var bodyPartRecords = source as BodyPartRecord[];
            if (bodyPartRecords == null)
            {
                bodyPartRecords = source.ToArray();
            }

            if (!bodyPartRecords.Any())
                return null;

            float Func(BodyPartRecord x)
            {
                return Mathf.Abs(FoodUtility.GetBodyPartNutrition(this, x) - nutritionWanted);
            }
            return bodyPartRecords.MinBy(Func);
        }
    }
}