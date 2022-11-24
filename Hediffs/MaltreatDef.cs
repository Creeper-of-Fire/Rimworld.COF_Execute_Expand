using System.Collections.Generic;
using COF_Torture.Things;
using Verse;

namespace COF_Torture.Hediffs
{
    public class MaltreatDef : HediffDef
    {
        public override void PostLoad()
        {
            base.PostLoad();
            if (this.descriptionHyperlinks != null && !this.descriptionHyperlinks.Contains(maltreat.enableByBuilding))
            {
                this.descriptionHyperlinks.Add(maltreat.enableByBuilding);
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
            else
                return labelAction;
        }
        public string GetDescriptionAction()
        {
            if (descriptionAction.NullOrEmpty())
                return description;
            else
                return descriptionAction;
        }
        public MaltreatProperties maltreat;
    }

    public class MaltreatProperties
    {
        public List<BodyPartGroupDef> ableBodyPartGroupDefs = new List<BodyPartGroupDef>();
        public ThingDef enableByBuilding = null;
    }
}