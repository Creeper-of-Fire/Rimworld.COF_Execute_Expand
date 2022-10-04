using System;
using System.Collections.Generic;
using System.Linq;
using COF_Torture.Hediffs;
using COF_Torture.ModSetting;
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
        public bool isSafe = true; //是否安全
        public Building_TortureBed_Def Def => (Building_TortureBed_Def)this.def;
        public Vector3 shiftPawnDrawPos => new Vector3(0, 0, Def.shiftPawnDrawPosZ);

        private List<Pawn> lastOwnerList;
        //private int CheckTicks;

        public Graphic graphic; //必定绘制
        public Graphic graphic_top;
        public Graphic graphic_top_using;
        public Graphic graphic_blood;
        public Graphic graphic_blood_top;
        public Graphic graphic_blood_top_using;
        public Texture2D texSafe;

        public override void Draw()
        {
            base.Draw();
            IntVec3 position = this.Position;
            Rot4 north = Rot4.North;
            Vector3 shiftedWithAltitude;
            shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.LayingPawn);
            //if (victim != null)
                //this.victim.DrawAt(shiftedWithAltitude);
            if (this.graphic == null)
            {
                trySetGraphic();
                return;
            }
            shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.Building);
            graphic.Draw(shiftedWithAltitude, Rot4.South, (Thing)this);
            if (isUsing)
            {
                //关上的盖子
                shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.PawnRope);
                graphic_top_using?.Draw(shiftedWithAltitude, north, (Thing)this);
            }
            else
            {
                //打开的盖子
                shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.Building);
                graphic_top?.Draw(shiftedWithAltitude, north, (Thing)this);
            }

            if (isUsed)
            {
                //底部的血液
                shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.BuildingOnTop);
                graphic_blood?.Draw(shiftedWithAltitude, north, (Thing)this);
                if (isUsing)
                {
                    //关上的盖子上的血液
                    shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.Projectile);
                    graphic_blood_top_using?.Draw(shiftedWithAltitude, north, (Thing)this);
                }
                else
                {
                    //打开的盖子上的血液
                    //shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.BuildingOnTop);
                    graphic_blood_top?.Draw(shiftedWithAltitude, north, (Thing)this);
                }
            }
        }

        public static void trySetGraphicSingle(Graphic gph, string texPath, Vector2 dS, ref Graphic graphic_change,
            bool isTrans = false)
        {
            if (graphic_change == null)
            {
                Shader shader = ShaderDatabase.Transparent;
                gph.path = texPath;
                var isExist = ContentFinder<Texture2D>.Get(gph.path, false);
                if (isExist != null)
                {
                    if (isTrans)
                        graphic_change = gph.GetColoredVersion(shader, new Color(1f, 1f, 1f,
                            ModSettingMain.Instance.Setting.topTransparency), Color.black);
                    else
                        graphic_change = gph.GetCopy(dS, null);
                }

                gph.path = texPath;
            }
        }

        public void trySetGraphic()
        {
            string texPath = this.Graphic.path;
            var dS = this.Graphic.drawSize;
            var gph = this.Graphic.GetCopy(dS, null);
            this.graphic  = this.Graphic.GetCopy(dS, null);;
            //trySetGraphicSingle(gph, texPath, dS, ref this.graphic);
            trySetGraphicSingle(gph, texPath + "_top", dS, ref this.graphic_top);
            trySetGraphicSingle(gph, texPath + "_top_using", dS, ref this.graphic_top_using, true);
            trySetGraphicSingle(gph, texPath + "_blood", dS, ref this.graphic_blood);
            trySetGraphicSingle(gph, texPath + "_blood_to", dS, ref this.graphic_blood_top);
            trySetGraphicSingle(gph, texPath + "_blood_top_using", dS, ref this.graphic_blood_top_using, true);
            texSafe = ContentFinder<Texture2D>.Get("COF_Torture/UI/isSafe");
        }

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
            Scribe_Values.Look<bool>(ref this.isSafe, "isSafe", defaultValue: ModSettingMain.Instance.Setting.isSafe);
        }


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            isSafe = ModSettingMain.Instance.Setting.isSafe;
            //this.Medical = false;
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
        }


        public override void TickRare()
        {
            base.TickRare();
            //这里会降低性能，其实不太好
            //CheckTicks++;
            //if (CheckTicks >= 60)
            //{
            //    CheckTicks = 0;
            if (!isUsing)
                return;
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

                //try
                //{
                foreach (var hediffR in victim.health.hediffSet.hediffs)
                {
                    if (hediffR != null)
                    {
                        if (hediffR is Hediff_ExecuteInjury it && it.giver != null)
                        {
                            it.giver = null;
                            //Log.Message("[COF_TORTURE]发现有主hediff" + hediffR + "已经去除主人");
                        }

                        if (hediffR is Hediff_WithGiver ht && ht.giver != null)
                            if (ht.giver == this)
                            {
                                ht.giver = null;
                                //Log.Message("[COF_TORTURE]发现没有被component.RemoveEffect去除的hediff" + ht + "。尝试去除");
                                //victim.health.RemoveHediff(hediffR);
                            }
                    }
                }
                //}
                /*catch
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
                }*/
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

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            if (Faction == Faction.OfPlayer)
            {
                var ro = new Command_Toggle();
                ro.defaultLabel = "CT_isSafe".Translate();
                ro.defaultDesc = "CT_isSafeDesc".Translate();
                ro.hotKey = KeyBindingDefOf.Misc3;
                ro.icon = texSafe;
                ro.isActive = () => isSafe;
                ro.toggleAction = () => isSafe = !isSafe;
                yield return ro;
            }
        }
    }
}