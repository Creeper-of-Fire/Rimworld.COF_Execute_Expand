using System.Collections.Generic;
using COF_Torture.Body;
using COF_Torture.Cell;
using COF_Torture.Hediffs;
using COF_Torture.Utility;
using COF_Torture.Utility.DefOf;
using JetBrains.Annotations;
using Verse;

namespace COF_Torture.Data
{
    public static class DataUtility
    {
        //[CanBeNull]
        public static PawnData GetPawnData(this Pawn pawn)
        {
            if (SaveStorage.StoreData == null)
            {
                if (Find.World != null)
                    SaveStorage.StoreData = Find.World.GetComponent<StoreData>();
                else
                {
                    ModLog.Error("错误，没有找到对应的世界");
                    return null;
                }
            }

            return SaveStorage.StoreData.GetPawnData(pawn);
        }

        [CanBeNull]
        private static CellEffectorData DirectGetCellEffectorData(Map map)
        {
            if (map == null)
                return null;
            var data = SaveStorage.CellEffectorDataDict.TryGetValue(map.uniqueID);
            //if (data != null)
            return data;

            /*else
            {
                data = map.GetComponent<CellEffectorData>();
                if (data == null)
                    data = new CellEffectorData(map);
                return data;
            }*/
        }

        /// <summary>
        /// 性能较差，请在不需要性能时使用
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        [CanBeNull]
        public static CellEffectorData GetCellEffectorData(Map map)
        {
            if (map == null)
                return null;
            var data = DirectGetCellEffectorData(map);
            if (data != null)
                return data;
            else
            {
                data = map.GetComponent<CellEffectorData>();
                //if (data == null)
                //    data = new CellEffectorData(map);
                return data;
            }
        }

        /// <summary>
        /// 只用于获取数据，而非设置内容
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        [CanBeNull]
        public static CellEffectorSet GetCellEffectors(IntVec3 pos, Map map) =>
            DirectGetCellEffectorData(map)?.GetCellEffector(pos);

        /// <summary>
        /// 只用于获取数据，而非设置内容
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        [CanBeNull]
        public static CellEffectorSet GetCellEffectors(this Pawn pawn) =>
            GetCellEffectors(pawn.Position, pawn.Map);
    }
}