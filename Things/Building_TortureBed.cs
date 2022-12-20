using System.Collections.Generic;
using System.Linq;
using COF_Torture.Component;
using COF_Torture.Data;
using COF_Torture.Dialog;
using COF_Torture.Jobs;
using COF_Torture.ModSetting;
using COF_Torture.Utility;
using RimWorld;
using UnityEngine;
using Verse;
using JobDefOf = RimWorld.JobDefOf;

namespace COF_Torture.Things
{
    public class Building_TortureBed : Building_Bed, ITortureThing
    {
        //public Pawn GetVictim() => victimAlive;

        private bool _inExecuteProgress;
        private Pawn _victim;
        public bool hasVictim => !victim.DestroyedOrNull();
        public bool inExecuteProgress => _inExecuteProgress;

        public void startExecuteProgress()
        {
            _inExecuteProgress = true;
        }
        /*ublic override Color DrawColor
        {
            get=>Color.white;
            set { return; }
        }*/

        public List<IWithThingGiver> hasGiven { get; set; } = new List<IWithThingGiver>();

        public Pawn victim
        {
            get => _victim;
            private set => _victim = value;
        }

        public void stopExecuteProgress()
        {
            _inExecuteProgress = false;
        }

        /// <summary>
        /// isUsed表示这个道具是否被使用过（指是否有人死在里面），会影响道具的图片显示
        /// </summary>
        public bool isUsed;

        /// <summary>
        /// 安全模式是否启用
        /// </summary>
        public bool isSafe = true;

        public Vector3 shiftPawnDrawPos
        {
            get
            {
                if (def is Building_TortureBedDef Def)
                    return new Vector3(0, 0, Def.shiftPawnDrawPosZ);
                return Vector3.zero;
            }
        }

        private List<Pawn> lastOwnerList;

        public bool showVictimBody = true;
        //private int CheckTicks;

        public Graphic graphic; //获取，但是不绘制，因为想不到怎么绘制比较好
        public Graphic graphic_top;
        public Graphic graphic_top_using;
        public Graphic graphic_blood;
        public Graphic graphic_blood_top;
        public Graphic graphic_blood_top_using;
        

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref _victim, "victim");
            Scribe_Values.Look(ref _inExecuteProgress, "_inExecuteProgress");
            Scribe_Values.Look(ref isUsed, "isUsed");
            Scribe_Values.Look(ref isSafe, "isSafe", defaultValue: ModSettingMain.Instance.Setting.isSecurityMode);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            isSafe = ModSettingMain.Instance.Setting.isSecurityMode;
            Medical = false;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            //如果床上面有囚犯
            if (hasVictim)
            {
                ReleaseVictim();
            }

            base.DeSpawn(mode);
        }

        public override void TickRare()
        {
            base.TickRare();
            //if (!isUsing)
            //    return;
            if (victim != null)
            {
                if (!victim.Dead)
                {
                    if (victim.jobs != null && victim.jobs.curJob.def == JobDefOf.Wait_Downed)
                    {
                        //Job job = JobMaker.MakeJob(COF_Torture.Jobs.JobDefOf.UseBondageAlone,
                        //    (LocalTargetInfo)(Verse.Thing)this);
                        //job.count = 1;
                        ModLog.Warning("被束缚的殖民者突然倒地，试图进行修复（很可能是殖民者被传送了或者" + this + "的所有者发生变更）");
                        Pawn_HealthTracker victimHealth = victim.health;
                        victim.jobs.ClearQueuedJobs();
                        victim.jobs.StopAll();
                        //RemoveVictim();
                        victim = victimHealth.hediffSet.pawn;
                        CT_Toils_GoToBed.BugFixBondageIntoBed(this, victim);
                    }
                }
            }
        }

        public void SetVictim(Pawn pawn)
        {
            SetVictimPlace(pawn);
            SetVictimHediff();
            //this.isUsing = true;

            void SetVictimPlace(Pawn p)
            {
                if (victim == null)
                {
                    victim = p;
                    lastOwnerList = new List<Pawn>(OwnersForReading);
                    OwnersForReading.Clear();
                    CompAssignableToPawn.TryAssignPawn(victim);
                    victim.GetPawnData().Fixer = this;
                }
            }

            void SetVictimHediff()
            {
                var crebb = GetComps<CompEffectForBondage>();
                if (crebb == null)
                {
                    ModLog.Error(this + " Can not find compEffectForBondage");
                    return;
                }

                foreach (var e in crebb)
                {
                    e.AddEffect(); //添加状态
                }
            }
        }

        public void ReleaseVictim()
        {
            _inExecuteProgress = false;
            showVictimBody = true;
            if (victim == null) return;
            TortureUtility.ShouldNotDie(victim);
            RemoveVictimHediff();
            if (TortureUtility.ShouldBeDead(victim)) //放下来时如果会立刻死，就改变死因为本comp造成
            {
                TortureUtility.KillVictimDirect(victim);
            }

            RemoveVictimPlace();
        }

        private void RemoveVictim()
        {
            RemoveVictimHediff();
            RemoveVictimPlace();
        }

        private void RemoveVictimPlace()
        {
            victim.GetPawnData().Fixer = null;
            CompAssignableToPawn.TryUnassignPawn(victim);
            victim = null;
            OwnersForReading.Clear();
            if (lastOwnerList != null)
                foreach (Pawn p in lastOwnerList)
                {
                    CompAssignableToPawn.TryAssignPawn(p);
                }
        }

        private void RemoveVictimHediff()
        {
            if (victim != null)
            {
                foreach (var hediffR in victim.health.hediffSet.hediffs)
                {
                    if (hediffR == null) continue;
                    if (!(hediffR is IWithThingGiver hg) || hg.Giver == null) continue;
                    if (hg.Giver == this)
                    {
                        hg.Giver = null;
                    }
                    //Log.Message("[COF_TORTURE]发现有主hediff" + hediffR + "已经去除主人");
                }

                victim.health.HealthTick(); //立即进行一次HediffTick以移除应当消失的Hediff
            }
            else
            {
                ModLog.Error(this + " Can not find its victim.");
                try
                {
                    TryRemoveHediffFromAllPawns();
                }
                catch
                {
                    ModLog.Message_Start("has no victim, and can't removeEffect.");
                }
            }
        }

        private void TryRemoveHediffFromAllPawns()
        {
            List<Pawn> allPawnsSpawned = Map.mapPawns.AllPawnsSpawned;
            ModLog.Message_Start("Try Remove Hediff From All Pawns.");
            foreach (var aps in allPawnsSpawned)
            {
                foreach (var hediffR in aps.health.hediffSet.hediffs)
                {
                    if (!(hediffR is IWithThingGiver hg) || hg.Giver == null) continue;
                    if (hg.Giver == this)
                    {
                        hg.Giver = null;
                    }
                }
            }
        }

        public override void Draw()
        {
            //base.Draw();
            IntVec3 position = Position;
            Rot4 north = Rot4.North;
            Vector3 shiftedWithAltitude;
            shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.LayingPawn);
            //if (victim != null)
            //    victim.DrawAt(shiftedWithAltitude+this.shiftPawnDrawPos);
            if (graphic == null)
            {
                trySetGraphic();
                return;
            }

            shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.Building);
            //graphic.Draw(shiftedWithAltitude, north, (Thing)this);
            if (inExecuteProgress)
            {
                //关上的盖子
                shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.PawnRope);
                graphic_top_using?.Draw(shiftedWithAltitude, north, this);
            }
            else
            {
                //打开的盖子
                shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.Building);
                graphic_top?.Draw(shiftedWithAltitude, north, this);
            }

            if (isUsed)
            {
                //底部的血液
                shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.BuildingOnTop);
                graphic_blood?.Draw(shiftedWithAltitude, north, this);
                if (inExecuteProgress)
                {
                    //关上的盖子上的血液
                    shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.Projectile);
                    graphic_blood_top_using?.Draw(shiftedWithAltitude, north, this);
                }
                else
                {
                    //打开的盖子上的血液
                    //shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.BuildingOnTop);
                    graphic_blood_top?.Draw(shiftedWithAltitude, north, this);
                }
            }
        }

        private static void trySetGraphicSingle(Graphic gph, string texPath, Vector2 dS, ref Graphic graphic_change,
            bool isMulti = false, bool isTrans = false, bool isBlood = false)
        {
            if (graphic_change != null)
                return;
            var texture2D = ContentFinder<Texture2D>.Get(texPath, false);
            if (texture2D == null)
                return;
            var shader = ShaderDatabase.CutoutComplex;
            var color = isBlood ? Color.white : gph.color;
            Graphic gph_temp;
            if (isMulti)
                gph_temp = GraphicDatabase.Get<Graphic_Multi>(texPath, shader, dS, color);
            else
                gph_temp = GraphicDatabase.Get<Graphic_Single>(texPath, shader, dS, color);
            graphic_change = gph_temp;
            if (isTrans)
            {
                color.a = ModSettingMain.Instance.Setting.topTransparency;
            }

            graphic_change = graphic_change.GetColoredVersion(ShaderDatabase.Transparent, color, Color.white);

            //Log.Message("1" + graphic_change + shader);
            //Log.Message("2");
        }

        private void trySetGraphic()
        {
            string texPath = Graphic.path;
            var dS = Graphic.drawSize;
            var gph = Graphic.GetCopy(dS, null);
            trySetGraphicFor6(gph, texPath, dS);
            //Log.Message("0");
            if (def.rotatable)
            {
                //Log.Message("1");
                trySetGraphicFor6(gph, texPath, dS, isRotatable: true);
            }
        }

        private void trySetGraphicFor6(Graphic gph, string texPath, Vector2 dS, bool isRotatable = true)
        {
            if (isRotatable)
                graphic = Graphic;
            else
                trySetGraphicSingle(gph, texPath + "_south", dS, ref graphic);
            trySetGraphicSingle(gph, texPath + "_top", dS, ref graphic_top,
                isMulti: isRotatable);
            trySetGraphicSingle(gph, texPath + "_top_using", dS, ref graphic_top_using,
                isTrans: true, isMulti: isRotatable);
            trySetGraphicSingle(gph, texPath + "_blood", dS, ref graphic_blood,
                isBlood: true, isMulti: isRotatable);
            trySetGraphicSingle(gph, texPath + "_blood_top", dS, ref graphic_blood_top,
                isBlood: true, isMulti: isRotatable);
            trySetGraphicSingle(gph, texPath + "_blood_top_using", dS, ref graphic_blood_top_using,
                isTrans: true, isBlood: true, isMulti: isRotatable);
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

        public override void DrawGUIOverlay()
        {
            if (Medical || Find.CameraDriver.CurrentZoom != CameraZoomRange.Closest ||
                !CompAssignableToPawn.PlayerCanSeeAssignments)
                return;
            Color defaultThingLabelColor = GenMapUI.DefaultThingLabelColor;
            if (!OwnersForReading.Any())
                GenMapUI.DrawThingLabel(this, "Unowned".Translate(), defaultThingLabelColor);
            else if (OwnersForReading.Count == 1 && !hasVictim)
            {
                if (OwnersForReading[0].InBed() && OwnersForReading[0].CurrentBed() == this)
                    return;
                GenMapUI.DrawThingLabel(this, OwnersForReading[0].LabelShort, defaultThingLabelColor);
            }
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                if (gizmo is Command command)
                {
                    if (command.defaultLabel == "CommandThingSetOwnerLabel".Translate() && hasVictim)
                    {
                        //gizmo.Disable("CT_CommandBondageBedDisableToSetWhenUsing".Translate());
                        continue;
                    }
                    if (command.defaultLabel == "CommandBedSetForPrisonersLabel".Translate() && hasVictim)
                    {
                        //gizmo.Disable("CT_CommandBondageBedDisableToSetWhenUsing".Translate());
                        continue;
                    }
                    if (command.defaultLabel == "CommandBedSetAsMedicalLabel".Translate())
                    {
                        continue;
                    }
                }

                if (ModsConfig.IdeologyActive && gizmo is Command_SetBedOwnerType && hasVictim)
                {
                    //gizmo.Disable("CT_CommandBondageBedDisableToSetWhenUsing".Translate());
                    continue;
                }

                yield return gizmo;
            }

            if (Faction == Faction.OfPlayer)
            {
                foreach (var com in this.Gizmo_SafeMode())
                {
                    yield return com;
                }

                if (victim != null)
                {
                    /*foreach (var com in this.Gizmo_StartAndStopExecute())
                    {
                        yield return com;
                    }*/

                    foreach (var com in this.Gizmo_AbuseMenu())
                    {
                        yield return com;
                    }

                    if (!ModSettingMain.Instance.Setting.isNoWayBack)
                    {
                        foreach (var com in this.Gizmo_ReleaseBondageBed())
                        {
                            yield return com;
                        }
                    }

                    if (ModSettingMain.Instance.Setting.controlMenuOn)
                    {
                        foreach (var com in this.Gizmo_TortureThingManager())
                        {
                            yield return com;
                        }
                    }
                }
            }
        }
    }
}