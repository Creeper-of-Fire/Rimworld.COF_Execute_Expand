using System;
using System.Collections.Generic;
using System.Linq;
using COF_Torture.Component;
using COF_Torture.Hediffs;
using COF_Torture.Jobs;
using COF_Torture.ModSetting;
using COF_Torture.Patch;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI;
using JobDefOf = RimWorld.JobDefOf;

namespace COF_Torture.Things
{
    public class Building_TortureBed : Building_Bed
    {
        private Pawn victimAlive;
        public Pawn GetVictim() => victimAlive;

        private bool isUsing; //isUsing只表示是否在被处刑使用，娱乐使用并不会触发它
        public bool isUnUsableForOthers() => isUsing;

        public bool isUsed; //isUsed表示这个道具是否被使用过（指是否有人死在里面），会影响道具的图片显示
        public bool isSafe = true; //是否安全

        /*public Vector3 shiftPawnDrawPos
        {
            get
            {
                if (def is Building_TortureBed_Def Def)
                    return new Vector3(0, 0, Def.shiftPawnDrawPosZ);
                else
                    return Vector3.zero;
            }
        }*/

        private List<Pawn> lastOwnerList;

        public bool showVictimBody = true;
        //private int CheckTicks;

        public Graphic graphic; //必定绘制
        public Graphic graphic_top;
        public Graphic graphic_top_using;
        public Graphic graphic_blood;
        public Graphic graphic_blood_top;
        public Graphic graphic_blood_top_using;
        public Texture2D texSafe;
        public Texture2D texPodEject;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Pawn>(ref this.victimAlive, "victim");
            Scribe_Values.Look<bool>(ref this.isUsing, "isUsing");
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
            if (isUnUsableForOthers())
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
            if (victimAlive != null)
            {
                if (victimAlive.Dead)
                {
                    
                }
                else
                {
                    if (victimAlive.jobs != null && victimAlive.jobs.curJob.def == JobDefOf.Wait_Downed)
                    {
                        //Job job = JobMaker.MakeJob(COF_Torture.Jobs.JobDefOf.UseBondageAlone,
                        //    (LocalTargetInfo)(Verse.Thing)this);
                        //job.count = 1;
                        Log.Message("[COF_TORTURE]被束缚的殖民者突然倒地，试图进行修复（很可能是殖民者被传送了或者" + this + "的所有者发生变更）");
                        Pawn_HealthTracker victimHealth = victimAlive.health;
                        victimAlive.jobs.ClearQueuedJobs();
                        victimAlive.jobs.StopAll();
                        //RemoveVictim();
                        victimAlive = victimHealth.hediffSet.pawn;
                        CT_Toils_GoToBed.BugFixBondageIntoBed(this, victimAlive);
                    }
                }
            }
        }

        public void SetVictim(Pawn pawn)
        {
            SetVictimPlace(pawn);
            SetVictimHediff();
            this.isUsing = true;

            void SetVictimPlace(Pawn p)
            {
                if (victimAlive == null)
                {
                    this.victimAlive = p;
                    lastOwnerList = new List<Pawn>(OwnersForReading);
                    OwnersForReading.Clear();
                    this.CompAssignableToPawn.TryAssignPawn(victimAlive);
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
            RemoveVictimHediff();
            KillVictimDirect(victimAlive);
            RemoveVictimPlace();

            void KillVictimDirect(Pawn pawn)
            {
                if (SettingPatch.RimJobWorldIsActive && pawn.story.traits.HasTrait(TraitDefOf.Masochist))
                {
                    var execute = Damages.DamageDefOf.Execute_Licentious;
                    var dInfo = new DamageInfo(execute, 1);
                    var dHediff = HediffMaker.MakeHediff(Hediffs.HediffDefOf.COF_Torture_Licentious, pawn);
                    pawn.Kill(dInfo, dHediff);
                }
                else
                {
                    var execute = Damages.DamageDefOf.Execute;
                    var dInfo = new DamageInfo(execute, 1);
                    var dHediff = HediffMaker.MakeHediff(Hediffs.HediffDefOf.COF_Torture_Fixed, pawn);
                    pawn.Kill(dInfo, dHediff);
                }
            }
        }


        public void ReleaseVictim()
        {
            this.isUsing = false;
            this.showVictimBody = true;
            if (victimAlive != null)
            {
                ShouldNotDie();
                if (HediffComp_ExecuteIndicator.ShouldBeDead(victimAlive)) //放下来时如果会立刻死，就改变死因为本comp造成
                {
                    KillVictim();
                }
                else
                    RemoveVictim();
            }
        }

        public void ShouldNotDie()
        {
            var bloodLoss = victimAlive.health.hediffSet.GetFirstHediffOfDef(RimWorld.HediffDefOf.BloodLoss);
            if (bloodLoss != null)
                if (bloodLoss.Severity > 0.9f)
                    bloodLoss.Severity = 0.9f;
        }

        private void RemoveVictim()
        {
            this.RemoveVictimHediff();
            this.RemoveVictimPlace();
        }

        private void RemoveVictimPlace()
        {
            this.CompAssignableToPawn.TryUnassignPawn(victimAlive);
            victimAlive = null;
            OwnersForReading.Clear();
            if (lastOwnerList != null)
                foreach (Pawn p in lastOwnerList)
                {
                    this.CompAssignableToPawn.TryAssignPawn(p);
                }
        }

        private void RemoveVictimHediff()
        {
            if (victimAlive != null)
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
                foreach (var hediffR in victimAlive.health.hediffSet.hediffs)
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
                                //victimAlive.health.hediffSet.hediffs.Remove(ht);
                            }
                    }
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
                            //aps.health.RemoveHediff(hT);
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
            graphic.Draw(shiftedWithAltitude, Rot4.South, (Thing)this);
            if (isUnUsableForOthers())
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
                if (isUnUsableForOthers())
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

                //Log.Message("2");
            }
        }

        private void trySetGraphic()
        {
            string texPath = this.Graphic.path;
            var dS = this.Graphic.drawSize;
            var gph = this.Graphic.GetCopy(dS, null);
            trySetGraphicFor5(gph, texPath, dS);
            gph.path = texPath;
            //Log.Message("0");
            if (this.def.rotatable)
            {
                string rot1 = null, rot2 = null;
                if (this.Rotation == Rot4.West)
                {
                    rot1 = "_west";
                    rot2 = "_east";
                }

                if (this.Rotation == Rot4.South)
                {
                    rot1 = "_south";
                    rot2 = "_north";
                }

                if (this.Rotation == Rot4.East)
                {
                    rot1 = "_east";
                    rot2 = "_west";
                }

                if (this.Rotation == Rot4.North)
                {
                    rot1 = "_north";
                    rot2 = "_south";
                }

                //Log.Message("1");
                trySetGraphicFor5(gph, texPath + rot2, dS);
                gph.path = texPath;
                trySetGraphicFor5(gph, texPath + rot1, dS);
                gph.path = texPath;
            }

            this.graphic = gph;
            texSafe = ContentFinder<Texture2D>.Get("COF_Torture/UI/isSafe");
            texPodEject = ContentFinder<Texture2D>.Get("COF_Torture/UI/PodEject");
        }

        private void trySetGraphicFor5(Graphic gph, string texPath, Vector2 dS)
        {
            trySetGraphicSingle(gph, texPath + "_top", dS, ref this.graphic_top);
            trySetGraphicSingle(gph, texPath + "_top_using", dS, ref this.graphic_top_using, true);
            trySetGraphicSingle(gph, texPath + "_blood", dS, ref this.graphic_blood);
            trySetGraphicSingle(gph, texPath + "_blood_to", dS, ref this.graphic_blood_top);
            trySetGraphicSingle(gph, texPath + "_blood_top_using", dS, ref this.graphic_blood_top_using, true);
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
            else if (this.OwnersForReading.Count == 1 && !this.isUsing)
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
                        if (isUnUsableForOthers())
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
                        if (isUnUsableForOthers())
                        {
                            gizmo.Disable("CT_CommandBondageBedDisableToSetWhenUsing".Translate());
                        }
                    }
                }

                if (ModsConfig.IdeologyActive && gizmo is Command_SetBedOwnerType)
                {
                    if (isUnUsableForOthers())
                    {
                        gizmo.Disable("CT_CommandBondageBedDisableToSetWhenUsing".Translate());
                    }
                }

                yield return gizmo;
            }

            if (Faction == Faction.OfPlayer)
            {
                var SafeMode = new Command_Toggle();
                SafeMode.defaultLabel = "CT_isSafe".Translate();
                SafeMode.defaultDesc = "CT_isSafeDesc".Translate();
                SafeMode.hotKey = KeyBindingDefOf.Misc4;
                SafeMode.icon = texSafe;
                SafeMode.isActive = () => isSafe;
                SafeMode.toggleAction = () => isSafe = !isSafe;
                yield return SafeMode;
                if (victimAlive != null)
                {
                    var Release = new Command_Action();
                    Release.defaultLabel = "CT_Release".Translate();
                    Release.defaultDesc = "CT_Release_BondageBed".Translate();
                    Release.hotKey = KeyBindingDefOf.Misc5;
                    Release.icon = texPodEject;
                    Release.action = this.ReleaseVictim;
                    yield return Release;
                }
            }
        }
    }
}