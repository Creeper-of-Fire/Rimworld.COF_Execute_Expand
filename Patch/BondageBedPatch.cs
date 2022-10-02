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
    //草泥马哪个傻逼写的这行代码导致我debug两个小时都没有发现错误的，哦原来是我啊
    /*[HarmonyPatch(typeof(RestUtility), "IsValidBedFor")]
    internal class RestUtility_IsValidBedFor
    {
        private static bool Prefix(ref bool __result, Verse.Thing bedThing, Pawn sleeper)
        {
            if (bedThing is Building_TortureBed)
            {
                if (sleeper.story.traits.HasTrait(TraitDefOf.Masochist))
                    return true;
            }
            __result = false;
            return false;
        }
    }*/
    //下面这个也不启用
}