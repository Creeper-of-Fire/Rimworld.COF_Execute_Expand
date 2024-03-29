using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace COF_Torture.Utility
{
    public class ActionWithBar: IExposable
    {
        private Action EndAction;
        private Pawn pawn;
        private Effecter effecter;
        public float progress;
        public string label;
        protected float progressPerTick;
        protected float offsetZ = -0.5f;

        public void ExposeData()
        {
            Scribe_Values.Look(ref EndAction,"endAction");
            Scribe_References.Look(ref pawn,"pawn");
            Scribe_Values.Look(ref effecter,"effecter");
            Scribe_Values.Look(ref progress,"progress");
            Scribe_Values.Look(ref label,"label");
            Scribe_Values.Look(ref progressPerTick,"progressPerTick");
            Scribe_Values.Look(ref offsetZ,"offsetZ");
        }

        public ActionWithBar()
        {
        }
        
        public ActionWithBar(Action endAction,Pawn pawn, float tick, string label = null)
        {
            EndAction = endAction;
            progressPerTick = 1 / tick;
            progress = 0f;
            this.label = label;
            this.pawn = pawn;
        }

        public void Tick()
        {
            if (effecter == null)
            {
                effecter = EffecterDefOf.ProgressBar.Spawn();
            }
            effecter.EffectTick((TargetInfo) (Thing) pawn, TargetInfo.Invalid);
            progress += progressPerTick;
            MoteProgressBar mote = ((SubEffecter_ProgressBar)effecter.children[0]).mote;
            if (mote == null)
                return;
            mote.progress = Mathf.Clamp01(progress);
            mote.offsetZ = offsetZ;
            mote.alwaysShow = true;
            //mote.DrawAt();
            if (progress >= 1f)
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
            effecter = null;
        }
    }
}