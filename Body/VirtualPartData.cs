using System.Collections.Generic;
using COF_Torture.Patch;
using Verse;

namespace COF_Torture.Body
{
    public interface IVirtualPart
    {
        
    }
    /// <summary>
    /// VirtualPartData才是真正的存放数据的地方
    /// </summary>
    public class VirtualPartData : IExposable, IVirtualPart
    {
        private Pawn Pawn;
        public float Sensitivity = 1;

        public VirtualPartData()
        {
        }

        public VirtualPartData(Pawn pawn) 
        {
            this.Pawn = pawn;
            RefreshVirtualParts();
        }

        /// <summary>
        /// 数据只存储在这里面
        /// </summary>
        private Dictionary<string, VirtualHediffSet> _virtualHediffByPart = new Dictionary<string, VirtualHediffSet>();

        public Dictionary<string, VirtualHediffSet> VirtualHediffByPart
        {
            get
            {
                if (_virtualHediffByPart.NullOrEmpty())
                {
                    _virtualHediffByPart = new Dictionary<string, VirtualHediffSet>();
                    //var HediffSet = new VirtualHediffSet();
                }

                return _virtualHediffByPart;
            }
        }

        [Unsaved(false)] private readonly Dictionary<BodyPartRecord, VirtualPartTree> _virtualTrees =
            new Dictionary<BodyPartRecord, VirtualPartTree>();

        public Dictionary<BodyPartRecord, VirtualPartTree> VirtualTrees
        {
            get
            {
                if (_virtualTrees.NullOrEmpty())
                    RefreshVirtualParts();
                return _virtualTrees;
            }
        }

        [Unsaved(false)] public readonly Dictionary<string, VirtualPartRecord> AllVirtualParts =
            new Dictionary<string, VirtualPartRecord>();

        public void RefreshVirtualParts()
        {
            if (!ModSetting.ModSettingMain.Instance.Setting.specificOrgans) return;
            if (!SettingPatch.RimJobWorldIsActive) return;
            _virtualTrees.Clear();
            AllVirtualParts.Clear();
            //var defs = DefDatabase<VirtualPartTreeDef>.AllDefs;
            var parts = Pawn.RaceProps.body.AllParts;

            foreach (var partRecord in parts)
            {
                if (_virtualTrees.ContainsKey(partRecord))
                    continue;
                var virtualPartTree = new VirtualPartTree(partRecord.def, Pawn);
                if (!virtualPartTree.defs.NullOrEmpty())
                {
                    _virtualTrees.SetOrAdd(partRecord, virtualPartTree);
                    foreach (var virtualPartRecord in virtualPartTree.AllParts)
                    {
                        AllVirtualParts.SetOrAdd(virtualPartRecord.UniLabel, virtualPartRecord);
                    }
                }
            }

            //ModLog.MessageEach(virtualParts.Values,
            //   tree => tree.defs.Select(def => def.label + tree.ToString()).FirstOrDefault());
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref Pawn, "pawn");
            Scribe_Collections.Look(ref _virtualHediffByPart, "VirtualHediffData", LookMode.Value, LookMode.Deep);
        }

        public VirtualPartRecord GetVirtualPartRecordByUniLabel(string label)
        {
            if (this.AllVirtualParts.ContainsKey(label))
                return this.AllVirtualParts[label];
            return null;
        }
    }
}