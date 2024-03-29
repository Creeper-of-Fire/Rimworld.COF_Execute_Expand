using System.Collections.Generic;
using COF_Torture.CellEffect;
using HugsLib;
using HugsLib.Utils;
using JetBrains.Annotations;
using RimWorld.Planet;
using Verse;

namespace COF_Torture.Data
{
    [StaticConstructorOnStartup]
    public class WorldStorage : ModBase
    {

        /// <summary>
        /// 需要存储的数据
        /// </summary>
        public static StoreData storeData;
        /// <summary>
        /// 临时生成的数据，不保存
        /// </summary>
        public static DesignData designData;

        public override void WorldLoaded()
        {
            base.WorldLoaded();
            storeData = UtilityWorldObjectManager.GetUtilityWorldObject<>()
        }

        public class StoreData : UtilityWorldObject
        {
            //private Dictionary<int, PawnData> _pawnDataDict;
            private Dictionary<int, PawnData> PawnDataDict = new Dictionary<int, PawnData>();
            private Dictionary<IntVec2, CellEffector> CellEffectorDict = new Dictionary<IntVec2, CellEffector>();

            public override void ExposeData()
            {
                if (Scribe.mode == LoadSaveMode.Saving)
                {
                    PawnDataDict.RemoveAll(
                        item =>
                        {
                            var pawn = item.Value;
                            if (pawn == null || !pawn.IsValid)
                                return true;
                            return false;
                        });
                }

                base.ExposeData();
                Scribe_Collections.Look(ref PawnDataDict, "COF_PawnData", LookMode.Value, LookMode.Deep);
                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    if (PawnDataDict == null)
                        PawnDataDict = new Dictionary<int, PawnData>();
                }
            }

            [CanBeNull]
            public PawnData GetPawnData(Pawn pawn)
            {
                bool flag = PawnDataDict.TryGetValue(pawn.thingIDNumber, out var pawnData);
                if (pawnData == null || !pawnData.IsValid)
                {
                    if (flag)
                        PawnDataDict.Remove(pawn.thingIDNumber);
                    pawnData = new PawnData(pawn);
                    PawnDataDict.Add(pawn.thingIDNumber, pawnData);
                }

                return pawnData;
            }

            public CellEffector GetCellEffector(IntVec2 pos)
            {
                bool flag = CellEffectorDict.TryGetValue(pos, out var cellEffectorData);
                if (cellEffectorData == null)
                {
                    if (flag)
                        CellEffectorDict.Remove(pos);
                    cellEffectorData = new CellEffector();
                    CellEffectorDict.Add(pos, cellEffectorData);
                }

                return cellEffectorData;
            }

            private void SetPawnData(Pawn pawn, PawnData data) => PawnDataDict.Add(pawn.thingIDNumber, data);

            public bool Empty() //TODO 这么写似乎不太好，需要持续添加每个属性
            {
                if (CellEffectorDict.NullOrEmpty() || PawnDataDict.NullOrEmpty())
                    return true;
                return false;
            }
        }
    }
}