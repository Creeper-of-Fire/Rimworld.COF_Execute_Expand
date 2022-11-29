using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace COF_Torture.Data
{
    public class DataStore : WorldComponent
    {
        public Dictionary<int, PawnData> PawnDatas = new Dictionary<int, PawnData>();

        public DataStore(World world)
            : base(world)
        {
        }

        public override void ExposeData()
        {
            /*if (Scribe.mode == LoadSaveMode.Saving)
                PawnData.RemoveAll(
                    item => item.Value == null || !item.Value.IsValid);*/
            base.ExposeData();
            Scribe_Collections.Look(ref PawnDatas, "COF_PawnData", LookMode.Value, LookMode.Deep);
            if (Scribe.mode != LoadSaveMode.LoadingVars || PawnDatas != null)
                return;
            PawnDatas = new Dictionary<int, PawnData>();
        }

        public PawnData GetPawnData(Pawn pawn)
        {
            bool flag = this.PawnDatas.TryGetValue(pawn.thingIDNumber, out var pawnData);
            if (pawnData == null || !pawnData.IsValid)
            {
                if (flag)
                    this.PawnDatas.Remove(pawn.thingIDNumber);
                pawnData = new PawnData(pawn);
                this.PawnDatas.Add(pawn.thingIDNumber, pawnData);
            }

            return pawnData;
        }

        private void SetPawnData(Pawn pawn, PawnData data) => this.PawnDatas.Add(pawn.thingIDNumber, data);
    }
}