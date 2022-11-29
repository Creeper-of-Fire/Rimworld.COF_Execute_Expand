using System;
using COF_Torture.Utility;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace COF_Torture.Patch
{
    [HarmonyPatch]
    public class MoreCorpseKindsPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn), "MakeCorpse",
            new Type[] { typeof(Building_Grave), typeof(Building_Bed) })]
        public static bool prefix(Pawn __instance, ref Corpse __result, Building_Grave assignedGrave,
            Building_Bed currentBed)
        {
            if (!__instance.health.hediffSet.HasHediff(Hediffs.HediffDefOf.COF_Torture_Barbecued))
            {
                if (__instance.genes == null) return true;
                if (__instance.genes != null && !__instance.genes.HasGene(Genes.GeneDefOf.COF_Torture_Barbecued)) return true;
            }

            if (__instance.story != null)
            {
                __instance.story.skinColorOverride = new Color(205, 97, 4);
                __instance.Drawer.renderer.graphics.ResolveAllGraphics();
                //__instance.story.SkinColorBase = new Color(5, 97, 4);
            }
            //Log.Message(""+__instance.story?.SkinColor);
            //Log.Message(""+__instance.story?.SkinColorOverriden);
            float a;
            if (currentBed != null)
                a = currentBed.Rotation.AsAngle;
            else
                a = 0.0f;
            __result = __instance.MakeCorpse_DifferentKind(assignedGrave, currentBed != null, a);
            return false;
        } //Todo
    }

    /*[HarmonyPatch]
    public class CorpseColorPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn_StoryTracker), "skinColorOverride", MethodType.Getter)]
        public static bool prefix(Pawn_StoryTracker __instance, ref Color __result)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (!pawn.health.hediffSet.HasHediff(Hediffs.HediffDefOf.COF_Torture_Barbecued) &&
                pawn.genes != null && !pawn.genes.HasGene(Genes.GeneDefOf.COF_Torture_Barbecued))
                return true;
            __result = new Color(5, 97, 4);
            return false;
        }
    }*/
}