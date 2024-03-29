namespace COF_Torture.Coitus
{
    public class CoitusPartLinker : ICoitusWithTransverseSizeDegree
    {
        public CoitusPart linkA;
        public CoitusPart linkB;
        public float diameter { get; set; }
        public float transverseSizeDegree { get; set; }
    }
}