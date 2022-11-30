using System.Collections.Generic;
using Verse;

namespace COF_Torture.Hediffs
{
    public class MaltreatDef : HediffDef
    {
        public override void PostLoad()
        {
            base.PostLoad();
            if (descriptionHyperlinks != null && !descriptionHyperlinks.Contains(maltreat.enableByBuilding))
            {
                descriptionHyperlinks.Add(maltreat.enableByBuilding);
            }
        }
        [MustTranslate]
        public string labelAction = "";
        [MustTranslate]
        public string descriptionAction = "";

        public string GetLabelAction()
        {
            if (labelAction.NullOrEmpty())
                return label;
            return labelAction;
        }
        public string GetDescriptionAction()
        {
            if (descriptionAction.NullOrEmpty())
                return description;
            return descriptionAction;
        }
        public MaltreatProperties maltreat;
    }

    public class MaltreatProperties
    {
        public List<BodyPartGroupDef> ableBodyPartGroupDefs = new List<BodyPartGroupDef>();
        public List<BodyPartDef> ableBodyPartDefs = new List<BodyPartDef>();
        public ThingDef enableByBuilding = null;
    }
}