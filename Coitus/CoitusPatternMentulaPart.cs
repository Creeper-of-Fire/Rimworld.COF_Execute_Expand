using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace COF_Torture.Coitus
{
    /// <summary>
    /// CoitusPart是关于Coitus内容的部件，包括人体也包括物品的可使用部分
    /// 
    /// </summary>
    public class CoitusPatternMentulaPart : CoitusPart
    {
        public Coitus_MentulaPatternData Mentula;

        public void Agere() //启动效果，包括Vagina和Mentula效果
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }

    public class Coitus_MentulaPatternData
    {
    }
}