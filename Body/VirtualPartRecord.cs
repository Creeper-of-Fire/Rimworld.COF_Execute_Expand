using System.Collections.Generic;
using COF_Torture.Data;
using Verse;

namespace COF_Torture.Body
{
    public class VirtualPartRecord : BodyPartRecord
    {
        [Unsaved(false)] public VirtualPartTree PartTree;

        public new List<VirtualPartRecord> parts = new List<VirtualPartRecord>();

        //[Unsaved(false)] private VirtualHediffSet _hediffSet = new VirtualHediffSet();
        [Unsaved(false)] private VirtualPartData Data;

        private VirtualHediffSet hediffSet
        {
            get
            {
                if (Data == null)
                {
                    Data = PartTree.pawn.GetPawnData().VirtualParts;
                }

                if (!Data.VirtualHediffByPart.ContainsKey(this.UniLabel))
                {
                    this.Data.VirtualHediffByPart.Add(this.UniLabel,new VirtualHediffSet());
                }
                var Set = Data.VirtualHediffByPart[this.UniLabel];
                return Set;
            }
        }

        public void AddHediff(Hediff hediff)
        {
            this.hediffSet.AddHediff(hediff);
        }

        public string UniLabel => GetUniLabel(this);

        public static string GetUniLabel(BodyPartRecord bodyPartRecord) =>
            bodyPartRecord.untranslatedCustomLabel + bodyPartRecord.def.defName;
    }
}