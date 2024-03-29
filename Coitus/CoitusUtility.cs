namespace COF_Torture.Coitus
{
    // public static class CoitusUtility
    // {
    //     public static float GetVolume(this CoitusPart coitusPart)
    //     {
    //         coitusPart.transverseSizeDegree
    //     }
    //
    //     public static float GetDiameter(this ICoitusWithTransverseSizeDegree coitusPart)
    //     {
    //         if 
    //         coitusPart.diameter=
    //     }
    // }

    public interface ICoitusWithTransverseSizeDegree
    {
        float transverseSizeDegree { get; set; }
        float diameter { get; set; }
    }

    public struct Geometry
    {
        public string name;


        public Geometry(string name)
        {
            this.name = name;
        }

        //球体
        public static Geometry sphere => new Geometry();

        //椭球体
        public static Geometry spheroid => new Geometry();

        //圆柱体
        public static Geometry cylinder => new Geometry();
    }
}