using System.Collections.Generic;
using System.Linq;
using COF_Torture.Component;
using COF_Torture.Jobs;
using COF_Torture.ModSetting;
using COF_Torture.Patch;
using RimWorld;
using UnityEngine;
using Verse;
using HarmonyLib;
using JobDefOf = RimWorld.JobDefOf;

namespace COF_Torture.Things
{
    public class Building_TortureBed : Building_Bed, ITortureThing
    {
        private Pawn _victim;
        //public Pawn GetVictim() => victimAlive;

        private bool _inExecuteProgress; //isUsing只表示是否在被处刑使用，娱乐使用并不会触发它
        public bool hasVictim => !_victim.DestroyedOrNull();

        public bool inExecuteProgress => _inExecuteProgress;

        public void startExecuteProgress()
        {
            _inExecuteProgress = true;
        }
        
        public Pawn victim => _victim;

        public void stopExecuteProgress()
        {
            _inExecuteProgress = false;
        }

        public bool isUsed; //isUsed表示这个道具是否被使用过（指是否有人死在里面），会影响道具的图片显示
        public bool isSafe = true; //是否安全

        public Vector3 shiftPawnDrawPos
        {
            get
            {
                if (def is Building_TortureBed_Def Def)
                    return new Vector3(0, 0, Def.shiftPawnDrawPosZ);
                else
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
            Scribe_References.Look<Pawn>(ref this._victim, "victim");
            Scribe_Values.Look<bool>(ref this._inExecuteProgress, "isProcessing");
            Scribe_Values.Look<bool>(ref this.isUsed, "isUsed");
            Scribe_Values.Look<bool>(ref this.isSafe, "isSafe", defaultValue: ModSettingMain.Instance.Setting.isSafe);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            isSafe = ModSettingMain.Instance.Setting.isSafe;
            this.Medical = false;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            //如果床上面有囚犯
            if (hasVictim)
            {
                this.ReleaseVictim();
            }

            base.DeSpawn(mode);
        }

        public override void TickRare()
        {
            base.TickRare();
            //if (!isUsing)
            //    return;
            if (_victim != null)
            {
                if (!_victim.Dead)
                {
                    if (_victim.jobs != null && _victim.jobs.curJob.def == JobDefOf.Wait_Downed)
                    {
                        //Job job = JobMaker.MakeJob(COF_Torture.Jobs.JobDefOf.UseBondageAlone,
                        //    (LocalTargetInfo)(Verse.Thing)this);
                        //job.count = 1;
                        Log.Message("[COF_TORTURE]被束缚的殖民者突然倒地，试图进行修复（很可能是殖民者被传送了或者" + this + "的所有者发生变更）");
                        Pawn_HealthTracker victimHealth = _victim.health;
                        _victim.jobs.ClearQueuedJobs();
                        _victim.jobs.StopAll();
                        //RemoveVictim();
                        _victim = victimHealth.hediffSet.pawn;
                        CT_Toils_GoToBed.BugFixBondageIntoBed(this, _victim);
                    }
                }
                else
                {
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
                if (_victim == null)
                {
                    this._victim = p;
                    lastOwnerList = new List<Pawn>(OwnersForReading);
                    OwnersForReading.Clear();
                    this.CompAssignableToPawn.TryAssignPawn(_victim);
                }
            }

            void SetVictimHediff()
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
        }

        public void KillVictim()
        {
            //RemoveVictimHediff();
            KillVictimDirect(_victim);
            //RemoveVictimPlace();

            void KillVictimDirect(Pawn pawn)
            {
                DamageDef execute;
                DamageInfo dInfo;
                Hediff dHediff;
                if (SettingPatch.RimJobWorldIsActive && pawn.story.traits.HasTrait(TraitDefOf.Masochist))
                {
                    execute = Damages.DamageDefOf.Execute_Licentious;
                    dHediff = HediffMaker.MakeHediff(Hediffs.HediffDefOf.COF_Torture_Licentious, pawn);
                }
                else
                {
                    execute = Damages.DamageDefOf.Execute;
                    dHediff = HediffMaker.MakeHediff(Hediffs.HediffDefOf.COF_Torture_Fixed, pawn);
                }

                dInfo = new DamageInfo(execute, 1);

                bool ShouldBeDeathrestingOrInComaInsteadOfDead(Pawn p)
                {
                    if (!ModsConfig.BiotechActive || p.genes == null || !p.genes.HasGene(GeneDefOf.Deathless))
                        return false;
                    BodyPartRecord brain = p.health.hediffSet.GetBrain();
                    return brain != null && !p.health.hediffSet.PartIsMissing(brain) &&
                           (double)p.health.hediffSet.GetPartHealth(brain) > 0.0;
                } //这里实际上是SanguophageUtility.ShouldBeDeathrestingOrInComaInsteadOfDead，但是因为ShouldBeDead被改过所以只能重写

                if (ShouldBeDeathrestingOrInComaInsteadOfDead(pawn))
                {
                    var ForceDeathrestOrComa = AccessTools.Method(typeof(Pawn_HealthTracker), "ForceDeathrestOrComa");
                    ForceDeathrestOrComa.Invoke(pawn.health, new object[] { (object)dInfo, (object)dHediff });
                }
                else
                {
                    if (pawn.Destroyed)
                        return;
                    pawn.Kill(dInfo, dHediff);
                }
            }
        }

        public void ReleaseVictim()
        {
            this._inExecuteProgress = false;
            this.showVictimBody = true;
            if (_victim != null)
            {
                HediffComp_ExecuteIndicator.ShouldNotDie(_victim);
                this.RemoveVictimHediff();
                if (HediffComp_ExecuteIndicator.ShouldBeDead(_victim)) //放下来时如果会立刻死，就改变死因为本comp造成
                {
                    KillVictim();
                }

                this.RemoveVictimPlace();
            }
        }

        private void RemoveVictim()
        {
            this.RemoveVictimHediff();
            this.RemoveVictimPlace();
        }

        private void RemoveVictimPlace()
        {
            this.CompAssignableToPawn.TryUnassignPawn(_victim);
            _victim = null;
            OwnersForReading.Clear();
            if (lastOwnerList != null)
                foreach (Pawn p in lastOwnerList)
                {
                    this.CompAssignableToPawn.TryAssignPawn(p);
                }
        }

        private void RemoveVictimHediff()
        {
            if (_victim != null)
            {
                /*var crebb = this.GetComps<COF_Torture.Component.CompEffectForBondage>();
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
                }*/


                //try
                //{
                foreach (var hediffR in _victim.health.hediffSet.hediffs)
                {
                    if (hediffR != null)
                    {
                        if (hediffR is IWithGiver hg && hg.Giver != null)
                        {
                            if (hg.Giver == this)
                            {
                                hg.Giver = null;
                            }
                            //Log.Message("[COF_TORTURE]发现有主hediff" + hediffR + "已经去除主人");
                        }
                    }
                }

                _victim.health.HealthTick();
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

        private void TryRemoveHediffFromAllPawns()
        {
            List<Pawn> allPawnsSpawned = this.Map.mapPawns.AllPawnsSpawned;
            Log.Message("[COF_TORTURE]Try Remove Hediff From All Pawns.");
            foreach (var aps in allPawnsSpawned)
            {
                foreach (var hediffR in aps.health.hediffSet.hediffs)
                {
                    if (hediffR is IWithGiver hg && hg.Giver != null)
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
            IntVec3 position = this.Position;
            Rot4 north = Rot4.North;
            Vector3 shiftedWithAltitude;
            shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.LayingPawn);
            //if (victim != null)
            //    victim.DrawAt(shiftedWithAltitude+this.shiftPawnDrawPos);
            if (this.graphic == null)
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
                if (inExecuteProgress)
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
            string texPath = this.Graphic.path;
            var dS = this.Graphic.drawSize;
            var gph = this.Graphic.GetCopy(dS, null);
            trySetGraphicFor6(gph, texPath, dS);
            //Log.Message("0");
            if (this.def.rotatable)
            {
                //Log.Message("1");
                trySetGraphicFor6(gph, texPath, dS, isRotatable: true);
            }
        }

        private void trySetGraphicFor6(Graphic gph, string texPath, Vector2 dS, bool isRotatable = true)
        {
            if (isRotatable)
                this.graphic = this.Graphic;
            else
                trySetGraphicSingle(gph, texPath + "_south", dS, ref this.graphic);
            trySetGraphicSingle(gph, texPath + "_top", dS, ref this.graphic_top,
                isMulti: isRotatable);
            trySetGraphicSingle(gph, texPath + "_top_using", dS, ref this.graphic_top_using,
                isTrans: true, isMulti: isRotatable);
            trySetGraphicSingle(gph, texPath + "_blood", dS, ref this.graphic_blood,
                isBlood: true, isMulti: isRotatable);
            trySetGraphicSingle(gph, texPath + "_blood_top", dS, ref this.graphic_blood_top,
                isBlood: true, isMulti: isRotatable);
            trySetGraphicSingle(gph, texPath + "_blood_top_using", dS, ref this.graphic_blood_top_using,
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
                !this.CompAssignableToPawn.PlayerCanSeeAssignments)
                return;
            Color defaultThingLabelColor = GenMapUI.DefaultThingLabelColor;
            if (!this.OwnersForReading.Any<Pawn>())
                GenMapUI.DrawThingLabel((Thing)this, (string)"Unowned".Translate(), defaultThingLabelColor);
            else if (this.OwnersForReading.Count == 1 && !this.hasVictim)
            {
                if (this.OwnersForReading[0].InBed() && this.OwnersForReading[0].CurrentBed() == this)
                    return;
                GenMapUI.DrawThingLabel((Thing)this, this.OwnersForReading[0].LabelShort, defaultThingLabelColor);
            }
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                if (gizmo is Command_Toggle ct)
                {
                    if (ct.defaultLabel == "CommandBedSetForPrisonersLabel".Translate())
                    {
                        if (hasVictim)
                        {
                            gizmo.Disable("CT_CommandBondageBedDisableToSetWhenUsing".Translate());
                        }
                    }

                    if (ct.defaultLabel == "CommandBedSetAsMedicalLabel".Translate())
                    {
                        continue;
                    }
                }

                if (gizmo is Command_Action ca)
                {
                    if (ca.defaultLabel == "CommandThingSetOwnerLabel".Translate())
                    {
                        if (hasVictim)
                        {
                            gizmo.Disable("CT_CommandBondageBedDisableToSetWhenUsing".Translate());
                        }
                    }
                }

                if (ModsConfig.IdeologyActive && gizmo is Command_SetBedOwnerType)
                {
                    if (hasVictim)
                    {
                        gizmo.Disable("CT_CommandBondageBedDisableToSetWhenUsing".Translate());
                    }
                }

                yield return gizmo;
            }
            if (ModSettingMain.Instance.Setting.controlMenuOn) yield break;

            if (Faction == Faction.OfPlayer)
            {
                foreach (var com in this.Gizmo_SafeMode())
                {
                    yield return com;
                }
                if (!ModSettingMain.Instance.Setting.isNoWayBack && _victim != null)
                {
                    foreach (var com in this.Gizmo_ReleaseBondageBed())
                    {
                        yield return com;
                    }
                }
            }
        }
    }
}