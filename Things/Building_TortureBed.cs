using System.Collections.Generic;
using System.Linq;
using COF_Torture.Hediffs;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace COF_Torture.Things
{
    public class Building_TortureBed : Building_Bed
    {
        public Pawn victim;
        public bool isUsing; //isUsing只表示是否在被处刑使用，娱乐使用并不会触发它
        public bool isUsed; //isUsed表示这个道具是否被使用过（指是否有人死在里面），会影响道具的图片显示

        private List<Pawn> lastOwnerList;
        //private int CheckTicks;

        public new bool Medical
        {
            get => false;
            set { return; }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Pawn>(ref this.victim, "victim");
            Scribe_Values.Look<bool>(ref this.isUsing, "isUsing");
            Scribe_Values.Look<bool>(ref this.isUsed, "isUsed");
        }
        
        

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            //this.Medical = false;
            /*if (this.def.IsBed)
                Log.Message(this + "is bed");
            else
                Log.Message(this + "is not bed");
            */
        }


        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            //如果床上面有囚犯
            if (victim != null)
            {
                this.RemoveVictim();
            }

            base.DeSpawn(mode);
            //TryRemoveHediffFromAllPawns();
        }

        public override void DrawGUIOverlay()
        {
            if (this.Medical || Find.CameraDriver.CurrentZoom != CameraZoomRange.Closest ||
                !this.CompAssignableToPawn.PlayerCanSeeAssignments)
                return;
            Color defaultThingLabelColor = GenMapUI.DefaultThingLabelColor;
            if (!this.OwnersForReading.Any<Pawn>())
                GenMapUI.DrawThingLabel((Thing)this, (string)"Unowned".Translate(), defaultThingLabelColor);
            else if (this.OwnersForReading.Count == 1 && !this.isUsing)
            {
                if (this.OwnersForReading[0].InBed() && this.OwnersForReading[0].CurrentBed() == this)
                    return;
                GenMapUI.DrawThingLabel((Thing)this, this.OwnersForReading[0].LabelShort, defaultThingLabelColor);
            }
            /*else
            {
                for (int index = 0; index < this.OwnersForReading.Count; ++index)
                {
                    if (!this.OwnersForReading[index].InBed() || this.OwnersForReading[index].CurrentBed() != this || !(this.OwnersForReading[index].Position == this.GetSleepingSlotPos(index)))
                        GenMapUI.DrawThingLabel((Vector2) this.GetMultiOwnersLabelScreenPosFor(index), this.OwnersForReading[index].LabelShort, defaultThingLabelColor);
                }
            }*/
        }
        /*private Vector3 AdjustOwnerLabelPosToAvoidOverlapping(Vector3 screenPos, int slotIndex)
        {
            Verse.Text.Font = GameFont.Tiny;
            float num1 = Verse.Text.CalcSize(this.OwnersForReading[slotIndex].LabelShort).x + 1f;
            Vector2 uiPosition = this.DrawPos.MapToUIPosition();
            float num2 = Mathf.Abs(screenPos.x - uiPosition.x);
            IntVec3 sleepingSlotPos = this.GetSleepingSlotPos(slotIndex);
            if ((double) num1 > (double) num2 * 2.0)
            {
                float num3 = slotIndex != 0 ? (float) this.GetSleepingSlotPos(0).x : (float) this.GetSleepingSlotPos(1).x;
                if ((double) sleepingSlotPos.x < (double) num3)
                    screenPos.x -= (float) (((double) num1 - (double) num2 * 2.0) / 2.0);
                else
                    screenPos.x += (float) (((double) num1 - (double) num2 * 2.0) / 2.0);
            }
            return screenPos;
        }
        private Vector3 GetMultiOwnersLabelScreenPosFor(int slotIndex)
        {
            IntVec3 sleepingSlotPos = this.GetSleepingSlotPos(slotIndex);
            Vector3 drawPos = this.DrawPos;
            if (this.Rotation.IsHorizontal)
            {
                drawPos.z = (float) sleepingSlotPos.z + 0.6f;
            }
            else
            {
                drawPos.x = (float) sleepingSlotPos.x + 0.5f;
                drawPos.z += -0.4f;
            }
            Vector2 screenPos = drawPos.MapToUIPosition();
            if (!this.Rotation.IsHorizontal && this.SleepingSlotsCount == 2)
                screenPos = (Vector2) this.AdjustOwnerLabelPosToAvoidOverlapping((Vector3) screenPos, slotIndex);
            return (Vector3) screenPos;
        }*/

        public override void TickRare()
        {
            base.TickRare();
            //这里会降低性能，其实不太好
            //CheckTicks++;
            //if (CheckTicks >= 60)
            //{
            //    CheckTicks = 0;
            if (isUsing)
            {
                if (victim != null)
                {
                    if (victim.Dead)
                    {
                        victim = null;
                        isUsing = false;
                    }
                    else
                    {
                        if (victim.jobs != null && victim.jobs.curJob.def == JobDefOf.Wait_Downed)
                        {
                            //Job job = JobMaker.MakeJob(COF_Torture.Jobs.JobDefOf.UseBondageAlone,
                            //    (LocalTargetInfo)(Verse.Thing)this);
                            //job.count = 1;
                            Log.Error("[COF_TORTURE]被束缚的殖民者突然倒地，试图进行修复（很可能是殖民者被传送了或者" + this + "的所有者发生变更）");
                            Pawn_HealthTracker victimHealth = victim.health;
                            victim.jobs.ClearQueuedJobs();
                            victim.jobs.StopAll();
                            //RemoveVictim();
                            victim = victimHealth.hediffSet.pawn;
                            BugFixBondageIntoBed(this, victim);
                        }
                    }
                }
                else
                {
                    Log.Error("[COF_TORTURE]机器" + this + "正在运行，理论上应该有被注册的使用者，实际上没有");
                    isUsing = false;
                }
                //}
            }
        }

        private static void BugFixBondageIntoBed(Building_Bed bed, Pawn takee)
        {
            if (bed.Destroyed)
            {
                takee.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
            else
            {
                Building_TortureBed thing = (Building_TortureBed)bed;
                thing.SetVictimPlace(takee);
                thing.SetVictimHediff(takee);
            }

            if (!bed.Destroyed)
            {
                takee.Position = RestUtility.GetBedSleepingSlotPosFor(takee, bed);
                takee.Notify_Teleported(false);
                takee.stances.CancelBusyStanceHard();
                takee.jobs.StartJob(JobMaker.MakeJob(JobDefOf.LayDown, (LocalTargetInfo)(Thing)bed),
                    JobCondition.InterruptForced, tag: new JobTag?(JobTag.TuckedIntoBed));
                takee.mindState.Notify_TuckedIntoBed();
            }

            LessonAutoActivator.TeachOpportunity(ConceptDefOf.PrisonerTab, (Thing)takee, OpportunityType.GoodToKnow);
        }

        public void SetVictim(Pawn pawn)
        {
            SetVictimPlace(pawn);
            SetVictimHediff(pawn);
            this.isUsing = true;
        }

        private void SetVictimPlace(Pawn pawn)
        {
            if (victim == null)
            {
                this.victim = pawn;
                lastOwnerList = new List<Pawn>(OwnersForReading);
                OwnersForReading.Clear();
                this.CompAssignableToPawn.TryAssignPawn(victim);
            }
        }

        private void SetVictimHediff(Pawn pawn)
        {
            var crebb = this.GetComps<COF_Torture.Component.CompEffectForBondage>();
            if (crebb == null)
            {
                Log.Error("[COF_TORTURE]" + this + " Can not find compEffectForBondage");
                return;
            }

            foreach (var e in crebb)
            {
                e.AddEffect(); //添加状态
            }
        }

        private void TryRemoveHediffFromAllPawns()
        {
            List<Pawn> allPawnsSpawned = this.Map.mapPawns.AllPawnsSpawned;
            Log.Message("[COF_TORTURE]Try Remove Hediff From All Pawns.");
            foreach (var aps in allPawnsSpawned)
            {
                foreach (var hediffR in aps.health.hediffSet.hediffs)
                {
                    if (hediffR is Hediff_WithGiver hT && hT.giver != null)
                        if (hT.giver == this)
                        {
                            hT.giver = null;
                            aps.health.RemoveHediff(hT);
                        }
                }
            }
        }

        public void RemoveVictim()
        {
            this.isUsing = false;
            this.TryRemoveVictimHediff();
            this.RemoveVictimPlace();
        }

        private void RemoveVictimPlace()
        {
            this.CompAssignableToPawn.TryUnassignPawn(victim);
            victim = null;
            OwnersForReading.Clear();
            if (lastOwnerList != null)
                foreach (Pawn p in lastOwnerList)
                {
                    this.CompAssignableToPawn.TryAssignPawn(p);
                }
        }

        private void TryRemoveVictimHediff()
        {
            if (victim != null)
            {
                try
                {
                    var crebb = this.GetComps<COF_Torture.Component.CompEffectForBondage>();
                    if (crebb == null)
                    {
                        Log.Error("[COF_TORTURE]" + this + " Can not find compEffectForBondage");
                    }
                    else
                    {
                        foreach (var e in crebb)
                        {
                            e.RemoveEffect(); //解除使用者
                        }

                        this.isUsing = false;
                    }
                }
                catch
                {
                    Log.Message("[COF_TORTURE]try move effect by component, but component can't removeEffect.");
                }

                try
                {
                    foreach (var hediffR in victim.health.hediffSet.hediffs)
                    {
                        if (hediffR != null)
                        {
                            if (hediffR is Hediff_WithGiver ht && ht.giver != null)
                                if (ht.giver == this)
                                {
                                    ht.giver = null;
                                    Log.Message("[COF_TORTURE]发现没有被component.RemoveEffect去除的hediff" + ht + "。尝试去除");
                                    victim.health.RemoveHediff(hediffR);
                                }
                        }
                    }
                }
                catch
                {
                    try
                    {
                        TryRemoveHediffFromAllPawns();
                    }
                    catch
                    {
                        Log.Message("[COF_TORTURE]try to remove all pawn's hediff, and can't remove Effect.");
                    }

                    Log.Message("[COF_TORTURE]has victim, but can't removeEffect.");
                }
            }
            else
            {
                Log.Error("[COF_TORTURE]" + this + " Can not find its victim.");
                try
                {
                    TryRemoveHediffFromAllPawns();
                }
                catch
                {
                    Log.Message("[COF_TORTURE]has no victim, and can't removeEffect.");
                }
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (AllComps != null)
            {
                for (int i = 0; i < AllComps.Count; i++)
                {
                    foreach (FloatMenuOption floatMenuOption in AllComps[i].CompFloatMenuOptions(myPawn))
                    {
                        yield return floatMenuOption;
                    }
                }
            }

            var a = base.GetFloatMenuOptions(myPawn);
            if (a != null)
            {
                var floatMenuOptions = a.ToList();
                foreach (var fMO in floatMenuOptions)
                {
                    yield return fMO;
                }
            }
        }
    }
}