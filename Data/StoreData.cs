using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace COF_Torture.Data
{
    public class StoreData : WorldComponent
    {
        private Dictionary<int, PawnData> PawnDataDict = new Dictionary<int, PawnData>();

        public StoreData(World world)
            : base(world)
        {
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
                PawnDataDict.RemoveAll(
                    item => item.Value == null || !item.Value.IsValid);
            base.ExposeData();
            Scribe_Collections.Look(ref PawnDataDict, "COF_PawnData", LookMode.Value, LookMode.Deep);
            if (Scribe.mode != LoadSaveMode.LoadingVars || PawnDataDict != null)
                return;
            PawnDataDict = new Dictionary<int, PawnData>();
        }

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

        private void SetPawnData(Pawn pawn, PawnData data) => PawnDataDict.Add(pawn.thingIDNumber, data);
    }
}