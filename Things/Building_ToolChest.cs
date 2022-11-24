using System.Collections.Generic;
using System.Linq;
using COF_Torture.Dialog;
using COF_Torture.Hediffs;
using COF_Torture.Jobs;
using COF_Torture.ModSetting;
using RimWorld;
using UnityEngine;
using Verse;
using JobDefOf = RimWorld.JobDefOf;

namespace COF_Torture.Things
{
    public class Building_ToolChest:Building
    {
        private readonly List<DefHyperlink> descriptionHyperlinks = new List<DefHyperlink>();
        public override IEnumerable<DefHyperlink> DescriptionHyperlinks {
            get
            {
                if (!this.descriptionHyperlinks.NullOrEmpty()) return descriptionHyperlinks.AsEnumerable();
                foreach (var hediffDef in DefDatabase<MaltreatDef>.AllDefs)
                {
                    if (hediffDef.maltreat.enableByBuilding == this.def)
                    {
                        this.descriptionHyperlinks.Add(hediffDef);
                    }
                }

                return descriptionHyperlinks.AsEnumerable();
            }
        }

        public override Color DrawColor
        {
            get=>Color.white;
            set { return; }
        }
    }
}