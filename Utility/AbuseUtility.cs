using System;
using System.Collections.Generic;
using COF_Torture.Body;
using COF_Torture.Data;
using COF_Torture.Hediffs;
using COF_Torture.Jobs;
using Verse;

namespace COF_Torture.Utility
{
    public static class AbuseUtility
    {
        private const float SeverityPerHour = 5f;
        private const int tickPerHour = 2500;

        public static void InitOneAction(this Dictionary<Hediff, ExposableList<ActionWithBar>> ActionList,
            MaltreatDef hediffDef, BodyPartRecord bodyPart, Pawn victim)
        {
            Action action;
            Hediff hediff;
            if (bodyPart is VirtualPartRecord vBodyPart)
            {
                var parentPart = vBodyPart.PartTree.parentPart;
                hediff = HediffMaker.MakeHediff(hediffDef, victim, parentPart);
                action = delegate
                {
                    if (!parentPart.IsMissingForPawn(victim))
                    {
                        victim.health.AddHediff(hediff, vBodyPart);
                    }
                };
            }
            else
            {
                hediff = HediffMaker.MakeHediff(hediffDef, victim, bodyPart);
                action = delegate
                {
                    if (!bodyPart.IsMissingForPawn(victim))
                        victim.health.AddHediff(hediffDef, bodyPart, dinfo: new DamageInfo());
                };
            }

            var tick = hediff.Severity / SeverityPerHour * tickPerHour;
            ActionList.DictListAdd(hediff,
                new ActionWithBar(action, victim, tick, bodyPart.Label + "," + hediffDef.GetLabelAction()));
        }

        public static void StartAbuseJob(Pawn victim, Pawn abuser, ITortureThing fixer)
        {
            if (fixer == null)
            {
                ModLog.Error("fixer为空");
                return;
            }
            if (victim == null)
            {
                ModLog.Error("victim为空");
                return;
            }
            if (abuser == null)
            {
                ModLog.Error("abuser为空");
                return;
            }
            if (!(fixer is Thing thing))
            {
                ModLog.Error("fixer不是Thing");
                return;
            }
            //Hediff_COF_Torture_IsAbusing.AddHediff_COF_Torture_IsAbusing(victim).Start();
            var job = JobMaker.MakeJob(JobDefOf.CT_DoMaltreat, (LocalTargetInfo)thing);
            job.count = 1;
            abuser.jobs.TryTakeOrderedJob(job);
        }
    }
}