using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace COF_Torture.Coitus
{
    /// <summary>
    /// 
    /// 
    /// </summary>
    public class CoitusVaginaPart : CoitusPart
    {

        public Coitus_VaginaPatternData Vagina;

        public void Agere() //启动效果，包括Vagina和Mentula效果
        {
            
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }

    /// <summary>
    /// 有四种类型：入口、通道、终点、无内部。这些类型是根据它“连接”的对象实时获得的。
    /// 有三个形态属性：长度和粗细等级（以及对应的已扩张量），还有具体形状；
    /// 两个存储属性：容积、表面积；
    /// 两个扩张属性：弹性（被张开的能力）、可塑性（恢复原状的能力）
    /// 它存储“连接”的方法是“连接口对象”
    /// </summary>
    public class Coitus_VaginaPatternData
    {
        public List<CoitusVaginaPart> links = new List<CoitusVaginaPart>();
        public CoitusLinkType coitusLinkType;

        public enum CoitusLinkType
        {
            End = 1, //腔道的底部
            Entrance = 2, //腔道的入口
            Corridor = 3, //通道部分
            Surface = 4, //表面
            Hidden = 5 //标志“未启用”的状态
        }

        public void UpdateCoitusLinkType()
        {
            this.coitusLinkType = UpdateCoitusLinkType_GetType();

            CoitusLinkType UpdateCoitusLinkType_GetType()
            {
                if (coitusLinkType == CoitusLinkType.Surface)
                    return CoitusLinkType.Surface; //Surface比较特殊，是通过其他函数来设定的
                if (links.Count == 0)
                    return CoitusLinkType.Hidden; //不与任何东西连接，则为隐藏
                if (links.Count == 1)
                    return CoitusLinkType.End; //如果Surface又只有一个东西相连，则为底部
                if (Enumerable.Any(links, link => link.Vagina.coitusLinkType == CoitusLinkType.Surface))
                    return CoitusLinkType.Entrance; //如果和外部相连，则入口
                return CoitusLinkType.Corridor; //其他情况
            }
        }
    }
}