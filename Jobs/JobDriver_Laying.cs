using System.Collections.Generic;
using COF_Torture.Things;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace COF_Torture.Jobs
{
    
    public class JobDriver_Laying : JobDriver
    {
        public Building_Bed Bed => this.job.GetTarget(TargetIndex.A).Thing as Building_Bed;

        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            this.Bed == null ||
            this.pawn.Reserve((LocalTargetInfo)(Thing)this.Bed, this.job, this.Bed.SleepingSlotsCount, 0,
                errorOnFailed: errorOnFailed);

        /*public override bool CanBeginNowWhileLyingDown()
        {
            return pawn.health.hediffSet.HasHediff(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Fixed);
        }*/


        protected override IEnumerable<Toil> MakeNewToils()
        {
            bool hasBed = this.Bed != null;
            yield return Toils_LayDown.LayDown(TargetIndex.A, hasBed, false,
                noBedLayingPosture: PawnPosture.LayingInBed);
        }

        /// <summary>
        /// 旋转
        /// </summary>
        public override Rot4 ForcedLayingRotation
        {
            get
            {
                Building_TortureBed thing = (Building_TortureBed)job.GetTarget(TargetIndex.A).Thing;
                if (thing != null)
                {
                    var d = thing.def as Building_TortureBed_Def;
                    if (d != null)
                        return d.pawnUsingRot;
                }

                return base.ForcedLayingRotation;
            }
        }

        //绘制偏移
        /*public override Vector3 ForcedBodyOffset
        {
            get
            {
                Building_TortureBed thing = (Building_TortureBed)job.GetTarget(TargetIndex.A).Thing;
                if (thing != null)
                {
                    var d = thing.def as Building_TortureBed_Def;
                    if (d != null)
                    {
                        //Log.Message("1");
                        return new Vector3(0, 0, d.shiftPawnDrawPosZ);
                    }
                }

                return base.ForcedBodyOffset;
            }
        }*/
    }
}