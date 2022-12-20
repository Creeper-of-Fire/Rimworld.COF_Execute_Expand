using System.Collections.Generic;
using COF_Torture.Cell;
using COF_Torture.Utility;
using HugsLib;
using RimWorld;
using Verse;

namespace COF_Torture.Data
{
    public class SaveStorage : ModBase
    {
        public static StoreData StoreData;
        public static Dictionary<int, CellEffectorData> CellEffectorDataDict = new Dictionary<int, CellEffectorData>();
        public override string ModIdentifier => "COF_Torture";

        public override void WorldLoaded()
        {
            StoreData = Find.World.GetComponent<StoreData>();
        }

        public override void MapLoaded(Map map)
        {
            CellEffectorDataDict.SetOrAdd(map.uniqueID, map.GetComponent<CellEffectorData>());
        }

        private SaveStorage()
        {
        }
    }
}