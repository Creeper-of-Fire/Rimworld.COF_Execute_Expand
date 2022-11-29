using Verse;

namespace COF_Torture.Data
{
    public static class PawnExtend
    {
        public static PawnData GetPawnData(this Pawn pawn)
        {
            if (SaveStorage.DataStore == null)
                SaveStorage.DataStore = new DataStore(Find.World);
            return SaveStorage.DataStore.GetPawnData(pawn);
        }
    }
}