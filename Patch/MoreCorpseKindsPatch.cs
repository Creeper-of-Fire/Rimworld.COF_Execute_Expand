using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace COF_Torture.Patch
{
    [HarmonyPatch]
    public class MoreCorpseKindsPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn), "MakeCorpse",
            new Type[] { typeof(Building_Grave), typeof(Building_Bed) })]
        public static bool prefix(Pawn __instance,ref Corpse __result, Building_Grave assignedGrave, Building_Bed currentBed)
        {
            float a;
            if (currentBed != null)
                a = currentBed.Rotation.AsAngle;
            else
                a = 0.0f;
            __result = __instance.MakeCorpse_DifferentKind(assignedGrave, currentBed != null, a);
            return false;
        }//Todo
    }
}