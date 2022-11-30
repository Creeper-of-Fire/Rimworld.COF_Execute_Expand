using RimWorld.Planet;

namespace COF_Torture.Data
{
    public class DesignatorsData : WorldComponent
    {
        public DesignatorsData(World world)
            : base(world)
        {
        }

        public void Update()
        {
            /*DesignatorsData.rjwHero = PawnsFinder.All_AliveOrDead.Where<Pawn>((Func<Pawn, bool>) (p => p.IsDesignatedHero())).ToList<Pawn>();
            DesignatorsData.rjwComfort = PawnsFinder.All_AliveOrDead.Where<Pawn>((Func<Pawn, bool>) (p => p.IsDesignatedComfort())).ToList<Pawn>();
            DesignatorsData.rjwService = PawnsFinder.All_AliveOrDead.Where<Pawn>((Func<Pawn, bool>) (p => p.IsDesignatedService())).ToList<Pawn>();
            DesignatorsData.rjwMilking = PawnsFinder.All_AliveOrDead.Where<Pawn>((Func<Pawn, bool>) (p => p.IsDesignatedMilking())).ToList<Pawn>();
            DesignatorsData.rjwBreeding = PawnsFinder.All_AliveOrDead.Where<Pawn>((Func<Pawn, bool>) (p => p.IsDesignatedBreeding())).ToList<Pawn>();
            DesignatorsData.rjwBreedingAnimal = PawnsFinder.All_AliveOrDead.Where<Pawn>((Func<Pawn, bool>) (p => p.IsDesignatedBreedingAnimal())).ToList<Pawn>();
        */
        }
    }
}