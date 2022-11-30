using System.Collections.Generic;
using System.Linq;
using COF_Torture.Body;
using COF_Torture.Hediffs;
using COF_Torture.ModSetting;
using HarmonyLib;
using RimWorld;
using Verse;

namespace COF_Torture.Patch
{
    [HarmonyPatch]
    public static class BodyPartWithGenderPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff",
            typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult))]
        public static void Prefix(Pawn_HealthTracker __instance, Hediff hediff, ref BodyPartRecord part)
        {
            if (part == null) return;
            var __setting = ModSettingMain.Instance.Setting;
            if (__setting.specificOrgansForAllHediffAdded && __setting.specificOrgans)
            {
                if (part is VirtualPartRecord __vPart)
                {
                    __vPart.AddVirtualHediff(hediff);
                    part = __vPart.PartTree.parentPart;
                }
                else
                {
                    __instance.AddVirtualHediff(hediff, part);
                }
            }
        }//对于虚拟部件，也可以直接使用AddHediff添加
    }
}