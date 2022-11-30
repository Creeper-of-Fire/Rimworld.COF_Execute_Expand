using System.Collections.Generic;
using System.Linq;
using COF_Torture.Hediffs;
using UnityEngine;
using Verse;

namespace COF_Torture.Things
{
    public class Building_ToolChest:Building
    {
        private readonly List<DefHyperlink> descriptionHyperlinks = new List<DefHyperlink>();
        public override IEnumerable<DefHyperlink> DescriptionHyperlinks {
            get
            {
                if (!descriptionHyperlinks.NullOrEmpty()) return descriptionHyperlinks.AsEnumerable();
                foreach (var hediffDef in DefDatabase<MaltreatDef>.AllDefs)
                {
                    if (hediffDef.maltreat.enableByBuilding == def)
                    {
                        descriptionHyperlinks.Add(hediffDef);
                    }
                }

                return descriptionHyperlinks.AsEnumerable();
            }
        }

        public override Color DrawColor => Color.white;
    }
}