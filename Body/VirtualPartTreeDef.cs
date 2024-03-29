using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace COF_Torture.Body
{
    /// <summary>
    /// VirtualPartTreeDef是像BodyPartDef一样可以无限嵌套延伸的def
    /// </summary>
    public class VirtualPartTreeDef : Def
    {
        public BodyPartDef parentPartDef;
        public List<VirtualPartRecord> parts;//包含了下一级的一系列子节点
        public Gender AbleGender = Gender.None;
    }
}