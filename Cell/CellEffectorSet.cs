using System.Collections.Generic;
using COF_Torture.Data;
using Verse;

namespace COF_Torture.Cell
{
    public class CellEffectorSet : IExposable
    {
        private List<CellEffector> cellEffectors = new List<CellEffector>();
        public IntVec2 Position;
        private Map map;
        public List<Pawn> pawns = new List<Pawn>();

        public CellEffectorSet()
        {
        }
        public CellEffectorSet(IntVec2 position, Map map)
        {
            this.Position = position;
            this.map = map;
        }

        public int Count()
        {
            return cellEffectors.Count;
        }

        public bool Empty()
        {
            return cellEffectors.NullOrEmpty();
        }

        /*public void SetID()
        {
            if (map != null && Position != default) 
                ID = "CellEffectorSet_" + Position + "_At_" + map.uniqueID;
        }*/

        public void ExposeData()
        {
            Scribe_Collections.Look(ref cellEffectors, "cellEffectors", LookMode.Deep);
            Scribe_Collections.Look(ref pawns,"pawns",LookMode.Reference);
            Scribe_Values.Look(ref Position,"Position");
            Scribe_References.Look(ref map,"map");
        }

        public void DoTick(Pawn pawn)
        {
            if (!pawns.Contains(pawn))
                pawns.Add(pawn);
            foreach (var cellEffector in cellEffectors)
            {
                cellEffector.DoTick(pawn);
                //ModLog.Message("DoEffect"+cellEffector);
            }
        }

        public void UndoEffect(Pawn pawn)
        {
            foreach (var cellEffector in cellEffectors)
            {
                cellEffector.TryUndoEffect(pawn);
                //cellEffector.ClearGiver();
                //cellEffector.CheckValid();
            }
        }

        public void AddEffector(CellEffector effector, Thing giver,int depth = 100)
        {
            depth--;
            if (depth <=0)
                return;
            effector.SetGiver(giver);
            if (cellEffectors.Contains(effector))
            {
                cellEffectors.Remove(effector);
                AddEffector(effector,giver,depth);
            }
            cellEffectors.Add(effector);
            //ModLog.Message(" "+effector+" "+giver);
        }

        public bool IsValid
        {
            get
            {
                List<Pawn> removePawnList = new List<Pawn>();
                foreach (var pawn in this.pawns)
                {
                    if (pawn.Position.ToIntVec2 != this.Position)
                    {
                        this.UndoEffect(pawn);
                        removePawnList.Add(pawn);
                    }
                }

                foreach (var pawn in removePawnList)
                {
                    this.pawns.Remove(pawn);
                }

                cellEffectors.RemoveAll(effector => !effector.CheckValid());
                if (this.cellEffectors.NullOrEmpty())
                    return false;
                return true;
            }
        }
    }
}