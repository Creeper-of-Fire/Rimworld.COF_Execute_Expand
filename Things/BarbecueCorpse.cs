using System;
using System.Collections.Generic;
using System.Linq;
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
            Scribe_References.Look<Pawn>(ref this.LastPawn, "LastPawn");
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
            BodyPartRecord bodyPartRecord = this.GetBestBodyPartToEat(ingester, nutritionWanted/TortureUtility.BBQNutritionFactor);
            if (bodyPartRecord == null)
            {
                Log.Error(ingester.ToString() + " ate " + (object)this +
                          " but no body part was found. Replacing with core part.");
                bodyPartRecord = this.InnerPawn.RaceProps.body.corePart;
            }

            float bodyPartNutrition = FoodUtility.GetBodyPartNutrition(this, bodyPartRecord);
            if (bodyPartRecord == this.InnerPawn.RaceProps.body.corePart)
            {
                if (PawnUtility.ShouldSendNotificationAbout(this.InnerPawn) && this.InnerPawn.RaceProps.Humanlike)
                    Messages.Message(
                        (string)"MessageEatenByPredator".Translate((NamedArgument)this.InnerPawn.LabelShort,
                            ingester.Named("PREDATOR"), this.InnerPawn.Named("EATEN")).CapitalizeFirst(),
                        (LookTargets)(Thing)ingester, MessageTypeDefOf.NegativeEvent);
                numTaken = 1;
            }
            else
            {
                Hediff_MissingPart hediffMissingPart =
                    (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, this.InnerPawn,
                        bodyPartRecord);
                hediffMissingPart.lastInjury = HediffDefOf.Bite;
                hediffMissingPart.IsFresh = true;
                this.InnerPawn.health.AddHediff((Hediff)hediffMissingPart);
                numTaken = 0;
            }

            nutritionIngested = bodyPartNutrition*TortureUtility.BBQNutritionFactor;
        }
        private BodyPartRecord GetBestBodyPartToEat(Pawn ingester, float nutritionWanted)
        {
            IEnumerable<BodyPartRecord> source = this.InnerPawn.health.hediffSet.GetNotMissingParts().Where<BodyPartRecord>((Func<BodyPartRecord, bool>) (x => x.depth == BodyPartDepth.Outside && (double) FoodUtility.GetBodyPartNutrition(this, x) > 1.0 / 1000.0));
            var bodyPartRecords = source as BodyPartRecord[];
            if (bodyPartRecords == null)
            {
                bodyPartRecords = source.ToArray();
            }

            if (!bodyPartRecords.Any<BodyPartRecord>())
                return (BodyPartRecord)null;
            else
            {
                float Func(BodyPartRecord x)
                {
                    return Mathf.Abs(FoodUtility.GetBodyPartNutrition(this, x) - nutritionWanted);
                }
                return bodyPartRecords.MinBy<BodyPartRecord, float>((Func<BodyPartRecord, float>)(Func));
            }
        }
    }
}