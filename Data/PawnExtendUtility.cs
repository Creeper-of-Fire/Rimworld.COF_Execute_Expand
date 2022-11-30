using System.Collections.Generic;
using COF_Torture.Body;
using COF_Torture.Hediffs;
using COF_Torture.Utility;
using COF_Torture.Utility.DefOf;
using Verse;

namespace COF_Torture.Data
{
    public static class PawnExtendUtility
    {
        public static PawnData GetPawnData(this Pawn pawn)
        {
            if (SaveStorage.DataStore == null)
                SaveStorage.DataStore = new DataStore(Find.World);
            return SaveStorage.DataStore.GetPawnData(pawn);
        }
    }
}