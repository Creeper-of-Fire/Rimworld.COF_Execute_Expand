using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace COF_Torture.Utility
{
    public class ActionWithBar
    {
        private Action EndAction;
        private Pawn pawn;
        private Effecter effecter;
        public float progress;
        public string label;
        protected float progressPerTick;
        protected float offsetZ = -0.5f;

        public ActionWithBar(Action endAction,Pawn pawn, float tick, string label = null)
        {
            this.EndAction = endAction;
            this.progressPerTick = 1 / tick;
            this.progress = 0f;
            this.label = label;
            this.pawn = pawn;
        }

        public void Tick()
        {
            if (effecter == null)
            {
                this.effecter = EffecterDefOf.ProgressBar.Spawn();
            }
            effecter.EffectTick((TargetInfo) (Thing) pawn, TargetInfo.Invalid);
            progress += progressPerTick;
            MoteProgressBar mote = ((SubEffecter_ProgressBar)effecter.children[0]).mote;
            if (mote == null)
                return;
            mote.progress = Mathf.Clamp01(progress);
            mote.offsetZ = offsetZ;
            mote.alwaysShow = true;
            mote.Draw();
            if (this.progress >= 1f)
                End();
        }

        public void End()
        {
            EndDirect();
            EndAction();
        }

        public void EndDirect()
        {
            if (effecter == null)
                return;
            effecter.Cleanup();
            effecter = (Effecter)null;
        }
    }
}