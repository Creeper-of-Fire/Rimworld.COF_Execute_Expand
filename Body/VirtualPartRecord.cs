using System.Collections.Generic;
using COF_Torture.Data;
using Verse;

namespace COF_Torture.Body
{
    /// <summary>
    /// VirtualPartRecord是用来和BodyPartRecord对接的，但是实际上内部的VirtualPartData才是本质，才是数据的存放位置
    /// </summary>
    public class VirtualPartRecord : BodyPartRecord
    {
        [Unsaved(false)] public VirtualPartTree PartTree;

        public new List<VirtualPartRecord> parts = new List<VirtualPartRecord>();
        
        [Unsaved(false)] private VirtualPartData Data;
        
        private VirtualHediffSet hediffSet
        {
            get
            {
                if (Data == null)
                {
                    Data = PartTree.pawn.GetPawnData()?.VirtualParts;
                }

                if (Data == null)
                {
                    ModLog.Error("错误，没有找到VirtualPartData");
                    return new VirtualHediffSet();
                }

                if (!Data.VirtualHediffByPart.ContainsKey(this.UniLabel))
                {
                    Data.VirtualHediffByPart.Add(this.UniLabel, new VirtualHediffSet());
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