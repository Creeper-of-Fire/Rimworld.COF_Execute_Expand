using System.Collections.Generic;
using COF_Torture.Data;
using Verse;

namespace COF_Torture.Body
{
    public class VirtualHediffSet: IExposable
    {
        public List<Hediff> hediffs = new List<Hediff>();
        public void ExposeData()
        {
            Scribe_Collections.Look(ref hediffs,"hediffs",LookMode.Reference);
        }
        
        public void AddHediff(Hediff hediff)
        {
            this.hediffs.Add(hediff);
            //ModLog.Message("Add"+hediff);
        }

        public Hediff FindFirstHediff(Hediff hediff)
        {
            foreach (var h in hediffs)
            {
                if (h == hediff)
                    return h;
            }

            return null;
        }
        public bool Contains(Hediff hediff)
        {
            return hediffs.Contains(hediff);
        }
    }
}