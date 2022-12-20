using System;
using COF_Torture.Data;
using COF_Torture.Utility;
using JetBrains.Annotations;
using Verse;

namespace COF_Torture.Cell
{
    public abstract class CellEffector : IExposable //, IWithThingGiver
    {
        //public CellEffectorDef def;
        private Thing _giver;

        [CanBeNull]
        public Thing Giver
        {
            get
            {
                if (CheckValid())
                    return _giver;
                else
                {
                    return null;
                }
            }
        }

        public CellEffector()
        {
        }

        public void ClearGiver()
        {
            _giver = null;
        }

        public void SetGiver(Thing value)
        {
            _giver = value;
        }

        /// <summary>
        /// 是否有效
        /// </summary>
        protected bool IsValid = true;

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool CheckValid()
        {
            if (_giver == null || _giver.Destroyed)
            {
                this.IsValid = false;
            }

            return IsValid;
        }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref _giver, "giver");
            //Scribe_Defs.Look<CellEffectorDef>(ref def, "def");
        }

        public virtual void DoTick(Pawn pawn)
        {
            //ModLog.Message(IsValid+""+pawn+"");
            if (!IsValid)
                return;
            if (pawn == null)
                return;
            PassEffect(pawn);
            if (pawn.pather != null && !pawn.pather.Moving)
                StayEffect(pawn);
        }

        /// <summary>
        /// 路过时的动作，只要经过就会触发
        /// </summary>
        public virtual void PassEffect(Pawn pawn)
        {
        }

        /// <summary>
        /// 仅限停留时的动作
        /// </summary>
        public virtual void StayEffect(Pawn pawn)
        {
        }

        /// <summary>
        /// 包含是否移除的逻辑判断
        /// </summary>
        public virtual void TryUndoEffect(Pawn pawn)
        {
        }
    }
}