using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace COF_Torture.Jobs
{
    public class CT_Toils_Laying
    {
        private const int TicksBetweenSleepZs = 100;
        private const int GetUpOrStartJobWhileInBedCheckInterval = 211;
        private const int SlavesInSleepingRoomCheckInterval = 2500;

        public static Toil LayDown(
            TargetIndex bedOrRestSpotIndex,
            bool hasBed,
            bool lookForOtherJobs,
            bool canSleep = true,
            bool gainRestAndHealth = true,
            PawnPosture noBedLayingPosture = PawnPosture.LayingInBed)
        {
            Toil layDown = new Toil();
            layDown.initAction = (Action)(() =>
            {
                Pawn actor = layDown.actor;
                actor.pather.StopDead();
                JobDriver curDriver = actor.jobs.curDriver;
                if (hasBed)
                {
                    Building_Bed thing = (Building_Bed)actor.CurJob.GetTarget(bedOrRestSpotIndex).Thing;
                    if (!thing.OccupiedRect().Contains(actor.Position))
                    {
                        Log.Error("Can't start LayDown toil because pawn is not in the bed. pawn=" + (object)actor);
                        actor.jobs.EndCurrentJob(JobCondition.Errored);
                        return;
                    }

                    actor.jobs.posture = PawnPosture.LayingInBed;
                    actor.mindState.lastBedDefSleptIn = thing.def;
                }
                else
                {
                    actor.jobs.posture = noBedLayingPosture;
                    actor.mindState.lastBedDefSleptIn = (ThingDef)null;
                }

                curDriver.asleep = false;
                if (actor.mindState.applyBedThoughtsTick == 0)
                {
                    actor.mindState.applyBedThoughtsTick = Find.TickManager.TicksGame + Rand.Range(2500, 10000);
                    actor.mindState.applyBedThoughtsOnLeave = false;
                }

                if (actor.ownership != null && actor.CurrentBed() != actor.ownership.OwnedBed)
                    ThoughtUtility.RemovePositiveBedroomThoughts(actor);
                actor.GetComp<CompCanBeDormant>()?.ToSleep();
            });
            layDown.tickAction = (Action)(() =>
            {
                Pawn actor = layDown.actor;
                Job curJob = actor.CurJob;
                JobDriver curDriver = actor.jobs.curDriver;
                Building_Bed thing = curJob.GetTarget(bedOrRestSpotIndex).Thing as Building_Bed;
                actor.GainComfortFromCellIfPossible();
                if (!curDriver.asleep)
                {
                    if (canSleep &&
                        (actor.needs.rest != null && (double)actor.needs.rest.CurLevel <
                            (double)RestUtility.FallAsleepMaxLevel(actor) || curJob.forceSleep))
                        curDriver.asleep = true;
                }
                else if (!canSleep)
                    curDriver.asleep = false;
                else if ((actor.needs.rest == null ||
                          (double)actor.needs.rest.CurLevel >= (double)RestUtility.WakeThreshold(actor)) &&
                         !curJob.forceSleep)
                    curDriver.asleep = false;

                if (curDriver.asleep && gainRestAndHealth && actor.needs.rest != null)
                {
                    float restEffectiveness =
                        thing == null || !thing.def.statBases.StatListContains(StatDefOf.BedRestEffectiveness)
                            ? StatDefOf.BedRestEffectiveness.valueIfMissing
                            : thing.GetStatValue(StatDefOf.BedRestEffectiveness);
                    actor.needs.rest.TickResting(restEffectiveness);
                }

                if (actor.mindState.applyBedThoughtsTick != 0 &&
                    actor.mindState.applyBedThoughtsTick <= Find.TickManager.TicksGame)
                {
                    ApplyBedThoughts(actor);
                    actor.mindState.applyBedThoughtsTick += 60000;
                    actor.mindState.applyBedThoughtsOnLeave = true;
                }

                if (actor.IsHashIntervalTick(100) && !actor.Position.Fogged(actor.Map))
                {
                    if (curDriver.asleep)
                        FleckMaker.ThrowMetaIcon(actor.Position, actor.Map, FleckDefOf.SleepZ);
                    if (gainRestAndHealth &&
                        actor.health.hediffSet.GetNaturallyHealingInjuredParts().Any<BodyPartRecord>())
                        FleckMaker.ThrowMetaIcon(actor.Position, actor.Map, FleckDefOf.HealingCross);
                }

                if (ModsConfig.IdeologyActive && thing != null && actor.IsHashIntervalTick(2500) && !actor.Awake() &&
                    (actor.IsFreeColonist || actor.IsPrisonerOfColony) && !actor.IsSlaveOfColony)
                {
                    Room room = thing.GetRoom();
                    if (!room.PsychologicallyOutdoors)
                    {
                        bool flag = false;
                        foreach (Building_Bed containedBed in room.ContainedBeds)
                        {
                            foreach (Pawn curOccupant in containedBed.CurOccupants)
                            {
                                if (curOccupant != actor && !curOccupant.Awake() && curOccupant.IsSlave &&
                                    !LovePartnerRelationUtility.LovePartnerRelationExists(actor, curOccupant))
                                {
                                    actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInRoomWithSlave);
                                    flag = true;
                                    break;
                                }
                            }

                            if (flag)
                                break;
                        }
                    }
                }

                if (actor.ownership != null && thing != null && !thing.Medical &&
                    !thing.OwnersForReading.Contains(actor))
                {
                    if (actor.Downed)
                        actor.Position = CellFinder.RandomClosewalkCellNear(actor.Position, actor.Map, 1);
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                else
                {
                    if (!lookForOtherJobs || !actor.IsHashIntervalTick(211))
                        return;
                    actor.jobs.CheckForJobOverride();
                }
            });
            layDown.defaultCompleteMode = ToilCompleteMode.Never;
            if (hasBed)
                layDown.FailOnBedNoLongerUsable(bedOrRestSpotIndex);
            layDown.AddFinishAction((Action)(() =>
            {
                Pawn actor = layDown.actor;
                JobDriver curDriver = actor.jobs.curDriver;
                if (actor.mindState.applyBedThoughtsOnLeave)
                    ApplyBedThoughts(actor);
                curDriver.asleep = false;
            }));
            return layDown;
        }

        private static void ApplyBedThoughts(Pawn actor)
        {
            if (actor.needs.mood == null)
                return;
            Building_Bed thing = actor.CurrentBed();
            actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBedroom);
            actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
            actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOutside);
            actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOnGround);
            actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInCold);
            actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInHeat);
            if (actor.GetRoom().PsychologicallyOutdoors)
                actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOutside);
            if (thing == null || thing.CostListAdjusted().Count == 0)
                actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptOnGround);
            if ((double)actor.AmbientTemperature <
                (double)actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin))
                actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInCold);
            if ((double)actor.AmbientTemperature >
                (double)actor.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax))
                actor.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleptInHeat);
            if (thing != null && thing == actor.ownership.OwnedBed && !thing.ForPrisoners &&
                !actor.story.traits.HasTrait(TraitDefOf.Ascetic))
            {
                ThoughtDef def = (ThoughtDef)null;
                if (thing.GetRoom().Role == RoomRoleDefOf.Bedroom)
                    def = ThoughtDefOf.SleptInBedroom;
                else if (thing.GetRoom().Role == RoomRoleDefOf.Barracks)
                    def = ThoughtDefOf.SleptInBarracks;
                if (def != null)
                {
                    int scoreStageIndex =
                        RoomStatDefOf.Impressiveness.GetScoreStageIndex(thing.GetRoom()
                            .GetStat(RoomStatDefOf.Impressiveness));
                    if (def.stages[scoreStageIndex] != null)
                        actor.needs.mood.thoughts.memories.TryGainMemory(
                            ThoughtMaker.MakeThought(def, scoreStageIndex));
                }
            }

            actor.Notify_AddBedThoughts();
        }
    }
}