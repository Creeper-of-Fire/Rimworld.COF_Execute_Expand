using COF_Torture.Utility;
using HugsLib;
using RimWorld;
using Verse;

namespace COF_Torture.Data
{
    public class SaveStorage : ModBase
    {
        public static DataStore DataStore;
        public static DesignatorsData DesignatorsData;

        public override string ModIdentifier => ModId;

        public static string ModId => "COF_Torture";

        public override void SettingsChanged() => ToggleTabIfNeeded();

        public override void WorldLoaded()
        {
            DataStore = Find.World.GetComponent<DataStore>();
            DesignatorsData = Find.World.GetComponent<DesignatorsData>();
            DesignatorsData.Update();
            ToggleTabIfNeeded();
            BodyPartFix();
        }

        private void BodyPartFix()
        {
            //foreach (var pawn in PawnsFinder.All_AliveOrDead) 
                //PawnExtendUtility.Notify_CheckGenderChange(pawn);
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