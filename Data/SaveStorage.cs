using HugsLib;
using Verse;

namespace COF_Torture.Data
{
    public class SaveStorage : ModBase
    {
        public static DataStore DataStore;
        public static DesignatorsData DesignatorsData;

        public override string ModIdentifier => SaveStorage.ModId;

        public static string ModId => "COF_Torture";

        public override void SettingsChanged() => this.ToggleTabIfNeeded();

        public override void WorldLoaded()
        {
            SaveStorage.DataStore = Find.World.GetComponent<DataStore>();
            SaveStorage.DesignatorsData = Find.World.GetComponent<DesignatorsData>();
            SaveStorage.DesignatorsData.Update();
            this.ToggleTabIfNeeded();
        }

        protected override bool HarmonyAutoPatch => false;

        private void ToggleTabIfNeeded()
        {
        }

        private SaveStorage()
        {
            
        }
    }
}