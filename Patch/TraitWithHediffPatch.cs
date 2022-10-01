using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace COF_Torture.Patch
{
    [HarmonyPatch(typeof(TraitSet))]
    [HarmonyPatch("GainTrait")]
    public static class GainTrait_Patch
    {
        private static void Postfix(Pawn ___pawn)
        {
            SpawnSetup_Patch.COF_Torture_AddPawn(___pawn);
        }
    }

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("SpawnSetup")]
    public static class SpawnSetup_Patch
    {
        private static void Postfix(Pawn __instance)
        {
            COF_Torture_AddPawn(__instance);
        }
        public static void COF_Torture_AddPawn(Pawn __instance)
        {
            try
            {
                if (__instance.story.traits.HasTrait(TraitDefOf.Masochist))
                {
                    if (__instance.health.hediffSet.GetFirstHediffOfDef(COF_Torture.Hediffs.HediffDefOf
                            .COF_Torture_SexualHeatWithPain) == null)
                    {
                        var hediff = HediffMaker.MakeHediff(COF_Torture.Hediffs.HediffDefOf
                            .COF_Torture_SexualHeatWithPain, __instance);
                        __instance.health.AddHediff(hediff);
                    }
                }
            }
            catch //(Exception ex)
            {
                //Log.Error($"[COF_TORTURE]Exception checking traits in {__instance}: {ex}");
            }
        }
    }
}