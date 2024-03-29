using System.Collections.Generic;
using System.Linq;
using COF_Torture.Data;
using COF_Torture.Utility;
using UnityEngine;
using Verse;

namespace COF_Torture.Body
{
    /// <summary>
    /// VirtualPartTree是用来解包的，专门对接VirtualPartTreeDef并且进行处理，然后对外提供处理结果的查找接口
    /// </summary>
    public class VirtualPartTree
    {
        public Pawn pawn;
        public List<VirtualPartTreeDef> defs = new List<VirtualPartTreeDef>();
        public BodyPartDef parentPartDef;

        public BodyPartRecord parentPart;

        public List<VirtualPartRecord> DirectSubParts
        {
            get
            {
                var list = new List<VirtualPartRecord>();
                foreach (var def in defs)
                {
                    list.AddRange(def.parts);
                }

                return list;
            }
        }
        //public Gender AbleGender => def.AbleGender;

        //public VirtualPartRecord CorePartRecord;
        private List<VirtualPartRecord> cachedAllParts = new List<VirtualPartRecord>();
        private List<VirtualPartRecord> cachedPartsVulnerableToFrostbite;

        public Dictionary<BodyPartTagDef, List<VirtualPartRecord>> cachedPartsByTag =
            new Dictionary<BodyPartTagDef, List<VirtualPartRecord>>();

        public Dictionary<BodyPartDef, List<VirtualPartRecord>> cachedPartsByDef =
            new Dictionary<BodyPartDef, List<VirtualPartRecord>>();

        public VirtualPartTree(BodyPartDef def, Pawn pawn)
        {
            this.parentPartDef = def;
            this.pawn = pawn;
            var AllDefs = DefDatabase<VirtualPartTreeDef>.AllDefs;
            foreach (var virtualPartTreeDef in AllDefs)
            {
                if (virtualPartTreeDef.parentPartDef == def)
                {
                    if (virtualPartTreeDef.AbleGender == Gender.Female && !pawn.IsFemale()) continue;
                    if (virtualPartTreeDef.AbleGender == Gender.Male && !pawn.IsMale()) continue;
                    this.defs.Add(virtualPartTreeDef);
                    //ModLog.Message(virtualPartTreeDef+"");
                }
            }

            this.BuildVirtualPartReferences();
        }

        public List<VirtualPartRecord> AllParts => this.cachedAllParts;

        public List<VirtualPartRecord> AllPartsVulnerableToFrostbite => this.cachedPartsVulnerableToFrostbite;

        public List<VirtualPartRecord> GetPartsWithTag(BodyPartTagDef tag)
        {
            if (!this.cachedPartsByTag.ContainsKey(tag))
            {
                this.cachedPartsByTag[tag] = new List<VirtualPartRecord>();
                foreach (var allPart in this.AllParts)
                {
                    if (allPart.def.tags.Contains(tag))
                        this.cachedPartsByTag[tag].Add(allPart);
                }
            }

            return this.cachedPartsByTag[tag];
        }

        public List<VirtualPartRecord> GetPartsWithDef(BodyPartDef partDef)
        {
            if (!this.cachedPartsByDef.ContainsKey(partDef))
            {
                this.cachedPartsByDef[partDef] = new List<VirtualPartRecord>();
                foreach (var allPart in this.AllParts)
                {
                    if (allPart.def == partDef)
                        this.cachedPartsByDef[partDef].Add(allPart);
                }
            }

            return this.cachedPartsByDef[partDef];
        }

        /// <summary>
        /// 不声明添加目标的情况下进行添加
        /// </summary>
        /// <param name="hediff"></param>
        public void AddHediff(Hediff hediff)
        {
            if (hediff == null) return;
            if (AllParts.NullOrEmpty()) return;
            AllParts.AsEnumerable()?.RandomElement()?.AddHediff(hediff);
        }

        public bool HasPartWithTag(BodyPartTagDef tag)
        {
            foreach (var t in this.AllParts)
            {
                if (t.def.tags.Contains(tag))
                    return true;
            }

            return false;
        }


        public void BuildVirtualPartReferences()
        {
            if (this.parentPart == null)
            {
                this.parentPart = this.pawn.RaceProps.body.AllParts.Find((record => record.def == parentPartDef));
            }
            //ModLog.Message("1");

            //if (this.corePart != null)
            foreach (var virtualPart in DirectSubParts)
            {
                this.CacheDataRecursive(virtualPart);
            }
            //ModLog.Message("2");

            this.cachedPartsVulnerableToFrostbite = new List<VirtualPartRecord>();
            List<VirtualPartRecord> allParts = this.AllParts;
            foreach (var t in allParts)
            {
                if ((double)t.def.frostbiteVulnerability > 0.0)
                    this.cachedPartsVulnerableToFrostbite.Add(t);
            }
        }

        private void CacheDataRecursive(VirtualPartRecord node)
        {
            if (node.def == null)
            {
                ModLog.Error("VirtualPart with null def. body=" + (object)this);
            }
            else
            {
                node.PartTree = this;
                foreach (var t in node.parts)
                    t.parent = node;

                node.coverageAbsWithChildren =
                    node.parent == null ? 1f : node.parent.coverageAbsWithChildren * node.coverage;
                float f = 1f;
                foreach (var t in node.parts)
                    f -= t.coverage;

                if ((double)Mathf.Abs(f) < 1E-05)
                    f = 0.0f;
                if ((double)f <= 0.0)
                    f = 0.0f;
                node.coverageAbs = node.coverageAbsWithChildren * f;
                if (node.height == BodyPartHeight.Undefined)
                    node.height = BodyPartHeight.Middle;
                if (node.depth == BodyPartDepth.Undefined)
                    node.depth = BodyPartDepth.Outside;
                foreach (var t in node.parts)
                {
                    if (t.height == BodyPartHeight.Undefined)
                        t.height = node.height;
                    if (t.depth == BodyPartDepth.Undefined)
                        t.depth = node.depth;
                }

                this.cachedAllParts.Add(node);
                foreach (var t in node.parts)
                    this.CacheDataRecursive(t);
            }
        }
    }
}