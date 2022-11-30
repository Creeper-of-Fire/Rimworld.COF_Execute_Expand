using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace COF_Torture.Body
{
    public class VirtualPartTreeDef : Def
    {
        public BodyPartDef parentPartDef;
        public List<VirtualPartRecord> parts;
        public Gender AbleGender = Gender.None;
    }
}