using System.Collections.Generic;
using Verse;

namespace COF_Torture.Coitus
{
    public class CoitusPartTracker : IExposable
    {
        public List<CoitusPart> totalCoitus;
        public List<CoitusPart> entranceCoitus;
        public List<CoitusPart> surfaceCoitus;

        public void ExposeData()
        {
            throw new System.NotImplementedException();
        }
    }
}