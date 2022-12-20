using System;
using System.Collections.Generic;
using System.Linq;
using COF_Torture.Data;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace COF_Torture.Cell
{
    public class CellEffectorData : MapComponent //TODO 没做垃圾处理
    {
        private CellEffectorSet[,] cellEffectorSets;
        //private int cellEffectorSetsCount;

        public CellEffectorData(Map map) : base(map)
        {
            //cellEffectorSetsCount = map.cellIndices.NumGridCells;
            cellEffectorSets = new CellEffectorSet[map.Size.x, map.Size.z];
        }

        [CanBeNull]
        public CellEffectorSet GetCellEffector(IntVec3 pos)
        {
            //ModLog.Message("x:"+cellEffectorSets.GetLength(0)+"z:"+cellEffectorSets.GetLength(1));
            return cellEffectorSets[pos.x, pos.z];
        }

        public int CellToInt(int intX, int intZ)
        {
            return this.map.cellIndices.CellToIndex(new IntVec3(intX, 0, intZ));
        }

        public (int, int) IntToCell(int index)
        {
            var Vec = this.map.cellIndices.IndexToCell(index);
            return (Vec.x, Vec.z);
        }

        public override void ExposeData()
        {
            /*if (Scribe.mode == LoadSaveMode.Saving)
            {
                for (var index = 0; index < cellEffectorSets.Length; index++)
                {
                    var cellEffectorSet = cellEffectorSets[index];
                    if (!cellEffectorSet.IsValid)
                        Array.Clear(cellEffectorSets, index, 1);
                }
            }*/

            base.ExposeData();


            var CellEffectorSetsSaveMode = new Dictionary<int, CellEffectorSet>();
            if (cellEffectorSets != null)
                for (var index0 = 0; index0 < cellEffectorSets.GetLength(0); index0++)
                for (var index1 = 0; index1 < cellEffectorSets.GetLength(1); index1++)
                {
                    var cellEffectorSet = cellEffectorSets[index0, index1];
                    if (cellEffectorSet != null && cellEffectorSet.IsValid)
                        CellEffectorSetsSaveMode.SetOrAdd(CellToInt(index0,index1), cellEffectorSet);
                }

            int countX;
            int countZ;
            if (map != null)
            {
                countX = map.Size.x;
                countZ = map.Size.z;
            }
            else
            {
                countX = 600;
                countZ = 600;
            }

            if (cellEffectorSets == null || cellEffectorSets.Length != countX * countZ)
            {
                cellEffectorSets = new CellEffectorSet[countX, countZ];
            }


            Scribe_Collections.Look(ref CellEffectorSetsSaveMode, "CellEffectorSets", LookMode.Value, LookMode.Deep);
            foreach (var pair in CellEffectorSetsSaveMode)
            {
                var Vec = IntToCell(pair.Key);
                if (pair.Value != null)
                    cellEffectorSets[Vec.Item1,Vec.Item2] = pair.Value;
            }
        }

        /// <summary>
        /// 添加效果到任意位置
        /// </summary>
        /// <param name="effector"></param>
        /// <param name="giver"></param>
        /// <param name="pos"></param>
        public void AddCellEffector(CellEffector effector, Thing giver, IntVec3 pos)
        {
            if (cellEffectorSets == null)
            {
                ModLog.Message("cellEffector不知道为什么为空");
                return;
            }

            if (cellEffectorSets[pos.x, pos.z] == null)
            {
                cellEffectorSets[pos.x, pos.z] = new CellEffectorSet(pos.ToIntVec2, map);
            }

            cellEffectorSets[pos.x, pos.z].AddEffector(effector, giver);
            //ModLog.Message(cellEffectorSets[pos.x, pos.z]+" "+effector+" "+giver);
        }

        /// <summary>
        /// 直接添加效果，添加位置就是物品所在位置
        /// </summary>
        /// <param name="effector"></param>
        /// <param name="giver"></param>
        public void AddCellEffector(CellEffector effector, Thing giver)
        {
            var pos = giver.Position;
            AddCellEffector(effector, giver, pos);
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager.TicksGame % 360 != 0)
                return;
            foreach (var set in cellEffectorSets)
            {
                if (set == null || set.pawns.NullOrEmpty())
                    continue;
                foreach (var pawn in set.pawns)
                {
                    if (pawn.Position.ToIntVec2 != set.Position)
                        set.UndoEffect(pawn);
                }
            }
        }
    }
}