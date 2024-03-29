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
    public class CoitusPart : IExposable, ICoitusWithTransverseSizeDegree
    {
        public float length;
        public float diameter { get; set; }
        public float transverseSizeDegree { get; set; }
        public Coitus_MatterContainedData Content;
        public CoitusPart relatedTo; //两个会一起动，比如说器官内部和它的表面

        public void Agere() //启动效果，包括Vagina和Mentula效果
        {
            
        }

        public virtual void ExposeData()
        {
            throw new System.NotImplementedException();
        }
    }

    

    public class Coitus_MatterContainedData
    {
    }
}