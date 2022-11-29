using Verse;

namespace COF_Torture.Data
{
    public static class ModLog
    {
        public static void Error(string message) => Log.Error("[" + SaveStorage.ModId + "] " + message);

        public static void Message(string message) => Log.Message("[" + SaveStorage.ModId + "] " + message);

        public static void Warning(string message) => Log.Warning("[" + SaveStorage.ModId + "] " + message);
    }
}