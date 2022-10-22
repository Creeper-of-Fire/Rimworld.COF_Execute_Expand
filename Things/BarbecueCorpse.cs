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
        /*public override Color DrawColor
        {
            get
            {
                var a = Color.yellow;
                a.a = 0.7f;
                return a;
            }
            set => this.SetColor(value);
        }*/
        /*public override IEnumerable<Thing> ButcherProducts(
            Pawn butcher,
            float efficiency)
        {
            foreach (Thing butcherProduct in this.InnerPawn.ButcherProducts(butcher, efficiency))
            {
                if (butcherProduct.def == InnerPawn.RaceProps.leatherDef)
                    continue;
                if (butcherProduct.def == InnerPawn.RaceProps.meatDef)
                {
                    butcherProduct.stackCount = (int)(butcherProduct.stackCount * TortureUtility.BBQNutritionFactor);
                    yield return butcherProduct;
                }

                yield return butcherProduct;
            }

            //if (this.InnerPawn.RaceProps.BloodDef != null)
            //    FilthMaker.TryMakeFilth(butcher.Position, butcher.Map, this.InnerPawn.RaceProps.BloodDef, this.InnerPawn.LabelIndefinite());
            if (this.InnerPawn.RaceProps.Humanlike)
            {
                Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.ButcheredHuman,
                    new SignalArgs(butcher.Named(HistoryEventArgsNames.Doer),
                        this.InnerPawn.Named(HistoryEventArgsNames.Victim))));
                TaleRecorder.RecordTale(TaleDefOf.ButcheredHumanlikeCorpse, (object)butcher);
            }
        }*/
        protected override void IngestedCalculateAmounts(
            Pawn ingester,
            float nutritionWanted,
            out int numTaken,
            out float nutritionIngested)
        {
            BodyPartRecord bodyPartRecord = this.GetBestBodyPartToEat(ingester, nutritionWanted);
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

            nutritionIngested = bodyPartNutrition;
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