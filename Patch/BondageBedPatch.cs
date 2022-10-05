using COF_Torture.Things;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace COF_Torture.Patch
{
    /*[HarmonyPatch(typeof(RestUtility), "IsValidBedFor")]
    internal class RestUtility_IsValidBedFor
    {
        private static bool Prefix(ref bool __result, Verse.Thing bedThing)
        {
            if (!(bedThing is Building_TortureBed))
                return true;
            __result = false;
            return false;
        }
    }*/
    //草泥马哪个傻逼写的上面这堆代码导致我debug两个小时都没有发现错误的，哦原来是我啊

    [HarmonyPatch]
    public static class PawnRenderer_GetBodyPos
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PawnRenderer), "GetBodyPos")]
        private static bool Prefix(PawnRenderer __instance, ref Vector3 __result, Vector3 drawLoc, out bool showBody)
        {
            var pawn = (Pawn)Traverse.Create(__instance).Field("pawn").GetValue();
            Building_Bed buildingBed = pawn.CurrentBed();
            if (buildingBed != null && buildingBed is Building_TortureBed tortureBed && tortureBed.isUnUsableForOthers())
            {
                AltitudeLayer AltLayer = buildingBed.def.altitudeLayer;
                drawLoc.y = AltLayer.AltitudeFor();
                __result = drawLoc;
                showBody = tortureBed.showVictimBody;
                return false;
            }
            showBody = buildingBed == null || buildingBed.def.building.bed_showSleeperBody;
            return true;
            /*Vector3 bodyPos;
            if (buildingBed != null && pawn.RaceProps.Humanlike)
            {
                showBody = buildingBed.def.building.bed_showSleeperBody;
                AltitudeLayer AltLayer = (AltitudeLayer)Mathf.Max((int)buildingBed.def.altitudeLayer, 18);
                IntVec3 position = pawn.Position;
                Vector3 shiftedWithAltitude;
                var vector3ShiftedWithAltitude = shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltLayer);
                vector3ShiftedWithAltitude.y += 0.023166021f;
                Rot4 rotation = buildingBed.Rotation;
                rotation.AsInt += 2;
                float num = -__instance.BaseHeadOffsetAt(Rot4.South).z;
                Vector3 vector3 = rotation.FacingCell.ToVector3() * num;
                bodyPos = shiftedWithAltitude + vector3;
                bodyPos.y += 0.008687258f;
            }
            else
            {
                showBody = true;
                bodyPos = drawLoc;
                if (pawn.ParentHolder is IThingHolderWithDrawnPawn parentHolder)
                    bodyPos.y = parentHolder.HeldPawnDrawPos_Y;
                else if (!pawn.Dead && pawn.CarriedBy == null)
                    bodyPos.y = AltitudeLayer.LayingPawn.AltitudeFor() + 0.008687258f;
            }

            __result = bodyPos;
            return false;*/
        }
    }
}