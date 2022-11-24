using System.Linq;
using COF_Torture.Things;
using RimWorld;
using Verse;

namespace COF_Torture.Component
{
    /*public class CompProperties_AffectedByToolChests : CompProperties_AffectedByFacilities
    {
        public override void PostLoadSpecial(ThingDef parent)
        {
            base.PostLoadSpecial(parent);
            var ToolChests = DefDatabase<ThingDef>.AllDefs.ToList()
                .FindAll(def => def.thingClass == typeof(Building_ToolChest));
            foreach (var toolChest in ToolChests)
            {
                this.linkableFacilities.Add(toolChest);
                parent.descriptionHyperlinks.Add(new DefHyperlink(toolChest));
            }
        }

        public CompProperties_AffectedByToolChests()
        {
            this.compClass = typeof(CompAffectedByFacilities);
        }
    }*/
}