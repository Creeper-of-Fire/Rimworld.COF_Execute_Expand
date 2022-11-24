using System;
using System.Collections.Generic;
using System.Linq;
using COF_Torture.Patch;
using COF_Torture.Things;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace COF_Torture
{
    [StaticConstructorOnStartup]
    public static class GizmoIcon
    {
        public static readonly Texture2D texSkull = ContentFinder<Texture2D>.Get("COF_Torture/UI/isSafe");
        public static readonly Texture2D texPodEject = ContentFinder<Texture2D>.Get("COF_Torture/UI/PodEject");
    }

    public static class TortureUtility
    {
        public const float satisfySexNeedWhenOrgasm = 0.2f;
        public const float BBQNutritionFactor = 1.4f;

        /// <summary>
        /// 角色高潮时自动调用本函数
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="OrgasmTimes"></param>
        public static void Orgasm(Pawn pawn, int OrgasmTimes = 1)
        {
            if (OrgasmTimes < 1)
                OrgasmTimes = 1;
            Hediff HediffOrgasm;
            bool hasOrgasmBefore;
            HediffComp_Disappears comps1 = null;
            if (pawn.health.hediffSet.HasHediff(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Orgasm))
            {
                HediffOrgasm =
                    pawn.health.hediffSet.GetFirstHediffOfDef(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Orgasm);
                hasOrgasmBefore = true;
            }
            else
            {
                HediffOrgasm = HediffMaker.MakeHediff(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Orgasm, pawn);
                hasOrgasmBefore = false;
            }

            for (int i = 0; i < OrgasmTimes; i++)
            {
                if (SettingPatch.RimJobWorldIsActive)
                {
                    Need need = pawn.needs.AllNeeds.Find((Predicate<Need>)(x => x.def == SettingPatch.SexNeed));
                    need.CurLevel += TortureUtility.satisfySexNeedWhenOrgasm; //因为高潮获得了性满足
                }

                if (hasOrgasmBefore)
                {
                    HediffOrgasm.Severity += 1;
                    if (comps1 == null)
                        comps1 = HediffOrgasm.TryGetComp<HediffComp_Disappears>();
                    comps1.ticksToDisappear = comps1.Props.disappearsAfterTicks.RandomInRange;
                }
                //补充高潮状态，重置消失时间
                else
                {
                    pawn.health.AddHediff(COF_Torture.Hediffs.HediffDefOf.COF_Torture_Orgasm);
                    hasOrgasmBefore = true;
                }
            }
        }

        public static Corpse MakeCorpse_DifferentKind(this Pawn pawn, Building_Grave assignedGrave, bool inBed,
            float bedRotation)
        {
            if (pawn.holdingOwner != null)
            {
                Log.Warning(
                    "We can't make corpse because the pawn is in a ThingOwner. Remove him from the container first. This should have been already handled before calling this method. holder=" +
                    (object)pawn.ParentHolder);
                return (Corpse)null;
            }

            var cD = pawn.RaceProps.corpseDef;
            SetCorpseDef(ref cD, pawn.def);

            Corpse corpse = (Corpse)ThingMaker.MakeThing(cD);
            corpse.InnerPawn = pawn;
            if (corpse is BarbecueCorpse cookedCorpse)
            {
                cookedCorpse.LastPawn = pawn;
            }

            if (assignedGrave != null)
                corpse.InnerPawn.ownership.ClaimGrave(assignedGrave);
            if (inBed)
                corpse.InnerPawn.Drawer.renderer.wiggler.SetToCustomRotation(bedRotation + 180f);
            return corpse;
        }

        public static void SetCorpseDef(ref ThingDef corpseDef, ThingDef pawnDef)
        {
            corpseDef.thingClass = typeof(BarbecueCorpse);
            corpseDef.SetStatBaseValue(StatDefOf.Beauty, 5.0f); //TODO 根据pawn的美丽程度修正美观程度
            corpseDef.SetStatBaseValue(StatDefOf.FoodPoisonChanceFixedHuman, 0.0f);
            corpseDef.SetStatBaseValue(StatDefOf.Nutrition, 5.2f * BBQNutritionFactor);
            corpseDef.defName = "CT_BarbecueCorpse_" + pawnDef.defName;
            corpseDef.label = (string)"BarbecueCorpseLabel".Translate((NamedArgument)pawnDef.label);
            corpseDef.description = (string)"BarbecueCorpseDesc".Translate((NamedArgument)pawnDef.label);
            //corpseDef.thingCategories.Remove(ThingCategoryDefOf.CorpsesHumanlike);
            //corpseDef.thingCategories.Add(Things.ThingCategoryDefOf.BarbecueCorpsesHumanlike);
            corpseDef.comps.Clear();
            corpseDef.comps.Add((CompProperties)new CompProperties_Forbiddable());
            corpseDef.ingestible = new IngestibleProperties();
            corpseDef.tickerType = TickerType.Rare;
            corpseDef.ingestible.parent = pawnDef;
            IngestibleProperties ingestible = corpseDef.ingestible;
            ingestible.foodType = FoodTypeFlags.Meal;
            ingestible.sourceDef = pawnDef;
            if (pawnDef.race.IsFlesh)
                ingestible.preferability = FoodPreferability.MealFine; //TODO 根据pawn的美丽程度修正味道
            else
                ingestible.preferability = FoodPreferability.NeverForNutrition;
            ingestible.maxNumToIngestAtOnce = 1;
            ingestible.ingestEffect = EffecterDefOf.EatMeat;
            ingestible.ingestSound = SoundDefOf.RawMeat_Eat;
            ingestible.tasteThought = ThoughtDefOf.AteFineMeal; // ThoughtDefOf.AteHumanlikeMeatAsIngredient;
            ingestible.specialThoughtDirect = null; //pawnDef.race.FleshType.ateDirect;
            ingestible.specialThoughtAsIngredient = null;
            ingestible.ateEvent = HistoryEventDefOf.AteHumanMeatAsIngredient;

            //ingestible.foodType = FoodTypeFlags.Corpse;
            //ingestible.sourceDef = thingDef1;
            //ingestible.preferability = thingDef1.race.IsFlesh ? FoodPreferability.DesperateOnly : FoodPreferability.NeverForNutrition;
            //DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef((object) ingestible, "tasteThought", ThoughtDefOf.AteCorpse.defName);
            //ingestible.maxNumToIngestAtOnce = 1;
            //ingestible.ingestEffect = EffecterDefOf.EatMeat;
            //ingestible.ingestSound = SoundDefOf.RawMeat_Eat;
            //ingestible.specialThoughtDirect = thingDef1.race.FleshType.ateDirect;
        }

        /// <summary>
        /// 往一个值是List的键值对的List里面添加值
        /// </summary>
        /// <param name="dict">字典</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <typeparam name="T">键的类型</typeparam>
        /// <typeparam name="V">值的类型</typeparam>
        public static void DictListAdd<T, V>(this Dictionary<T, List<V>> dict, T key, V value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key].Add(value);
            }
            else
            {
                var tempList = new List<V> { value };
                dict.Add(key, tempList);
            }
        }

        /// <summary>
        /// 绕过ShouldBeDead，直接杀死某人（会处理Deathrest），并且会给Kill函数注入本模组专有的死亡讯息
        /// </summary>
        /// <param name="pawn">要杀死的Pawn</param>
        public static void KillVictimDirect(Pawn pawn)
        {
            if (pawn.Dead)
                Log.Error("try to kill a dead pawn");
            DamageDef execute;
            Hediff dHediff;
            if (SettingPatch.RimJobWorldIsActive && pawn.story.traits.HasTrait(TraitDefOf.Masochist))
            {
                execute = Damages.DamageDefOf.Execute_Licentious;
                dHediff = HediffMaker.MakeHediff(Hediffs.HediffDefOf.COF_Torture_Licentious, pawn);
            }
            else
            {
                execute = Damages.DamageDefOf.Execute;
                dHediff = HediffMaker.MakeHediff(Hediffs.HediffDefOf.COF_Torture_Fixed, pawn);
            }

            var dInfo = new DamageInfo(execute, 1);

            bool ShouldBeDeathrestingOrInComaInsteadOfDead(Pawn p)
            {
                if (!ModsConfig.BiotechActive || p.genes == null || !p.genes.HasGene(GeneDefOf.Deathless))
                    return false;
                BodyPartRecord brain = p.health.hediffSet.GetBrain();
                return brain != null && !p.health.hediffSet.PartIsMissing(brain) &&
                       (double)p.health.hediffSet.GetPartHealth(brain) > 0.0;
            } //这里实际上是SanguophageUtility.ShouldBeDeathrestingOrInComaInsteadOfDead，但是因为ShouldBeDead被改过所以只能重写

            if (ShouldBeDeathrestingOrInComaInsteadOfDead(pawn))
            {
                var ForceDeathrestOrComa = AccessTools.Method(typeof(Pawn_HealthTracker), "ForceDeathrestOrComa");
                ForceDeathrestOrComa.Invoke(pawn.health, new object[] { (object)dInfo, (object)dHediff });
            }
            else
            {
                if (pawn.Destroyed)
                    return;
                pawn.Kill(dInfo, dHediff);
            }
        }

        /// <summary>
        /// 因为原版的ShouldBeDead被HarmonyPatch绕过了，所以重新写一个ShouldBeDead
        /// </summary>
        public static bool ShouldBeDead(Pawn pawn)
        {
            var health = pawn.health;
            if (health.Dead)
                return true;
            for (int index = 0; index < health.hediffSet.hediffs.Count; ++index)
            {
                if (health.hediffSet.hediffs[index].CauseDeathNow())
                    return true;
            }

            if (health.ShouldBeDeadFromRequiredCapacity() != null)
                return true;
            if ((double)PawnCapacityUtility.CalculatePartEfficiency(health.hediffSet, pawn.RaceProps.body.corePart) <=
                0.0)
            {
                if (DebugViewSettings.logCauseOfDeath)
                    Log.Message("CauseOfDeath: zero efficiency of " + pawn.RaceProps.body.corePart.Label);
                return true;
            }

            return health.ShouldBeDeadFromLethalDamageThreshold();
        }

        /// <summary>
        /// 处理Pawn的状态使它暂时不会死
        /// </summary>
        /// <param name="pawn">pawn</param>
        public static void ShouldNotDie(Pawn pawn)
        {
            var bloodLoss = pawn.health.hediffSet.GetFirstHediffOfDef(RimWorld.HediffDefOf.BloodLoss);
            if (bloodLoss != null)
                if (bloodLoss.Severity > 0.9f)
                    bloodLoss.Severity = 0.9f;
        }

        /// <summary>
        /// 原本的BodyPartGroup是层层嵌套的，这个算法把嵌套解开，使得一个BodyPart只对应一个BodyPartGroup。/n这个算法同样适用于其他情况
        /// </summary>
        /// <param name="allParts">键：BodyPartGroup，值：其包含的BodyPart列表，彼此重复。</param>
        /// <typeparam name="T">BodyPartGroup</typeparam>
        /// <typeparam name="V">BodyPart</typeparam>
        /// <returns>键：BodyPartGroup，值：其包含的BodyPart列表，互不重复。</returns>
        public static Dictionary<T, List<V>> untieNestedDict<T, V>(Dictionary<T, List<V>> allParts)
        {
            var fullGroups = new Dictionary<T, List<V>>();
            var regroupedGroups = new Dictionary<T, List<V>>();
            //var unfiledGroups = new List<T>();
            var unfiledParts = new List<V>(); //非引用而是复制
            foreach (var bodyPartGroup in allParts)
            {
                foreach (var bodyPart in bodyPartGroup.Value)
                {
                    fullGroups.DictListAdd(bodyPartGroup.Key, bodyPart);
                    unfiledParts.Add(bodyPart);
                }
                //unfiledGroups.Add(bodyPartGroup.Key);
            }

            unfiledParts = unfiledParts.Distinct().ToList();
            ReGroup();
            return regroupedGroups;

            void ReGroup()
            {
                var minGroup = GetMinBodyPartGroup(fullGroups);
                foreach (var bodyPart in minGroup.Value)
                {
                    if (unfiledParts.Contains(bodyPart))
                        //目的是分类bodyPart到Group里面，unfiledParts里面的就是没有分类的bodyPart
                    {
                        unfiledParts.Remove(bodyPart);
                        regroupedGroups.DictListAdd(minGroup.Key, bodyPart);
                    }
                }

                fullGroups.Remove(minGroup.Key);
                if (fullGroups.NullOrEmpty())
                    return;
                ReGroup();
            }

            KeyValuePair<T, List<V>> GetMinBodyPartGroup(
                Dictionary<T, List<V>> Groups)
            {
                var minGroup = new KeyValuePair<T, List<V>>();
                foreach (var Group in Groups)
                {
                    if (minGroup.Value.NullOrEmpty())
                    {
                        minGroup = Group;
                        continue;
                    }

                    if (Group.Value.Count < minGroup.Value.Count)
                    {
                        minGroup = Group;
                    }
                }

                return minGroup;
            }
        }

        /// <summary>
        /// 转换列表内部数据的类型
        /// </summary>
        /// <param name="list1">原列表</param>
        /// <typeparam name="T1">原类型</typeparam>
        /// <typeparam name="T2">目标类型</typeparam>
        /// <returns>转换后列表</returns>
        public static List<T2> ListReform<T1, T2>(this List<T1> list1)
        {
            var list2 = new List<T2>();
            foreach (var value1 in list1)
            {
                if (value1 is T2 value2)
                    list2.Add(value2);
            }

            return list2;
        }
    }

    /// <summary>
    /// 接口，需要实现设置处刑对象和执行处刑
    /// </summary>
    public interface ITortureThing
    {
        /// <summary>
        /// 释放处刑对象
        /// </summary>
        void ReleaseVictim();

        /// <summary>
        /// 设置处刑对象
        /// </summary>
        /// <param name="pawn">处刑对象</param>
        void SetVictim(Pawn pawn);

        /// <summary>
        /// 判断是否在执行处刑，注意，只表示是否在被处刑使用，娱乐使用并不会触发它
        /// </summary>
        bool inExecuteProgress { get; }

        /// <summary>
        /// 开始处刑
        /// </summary>
        void startExecuteProgress();

        /// <summary>
        /// 暂停处刑
        /// </summary>
        void stopExecuteProgress();

        List<IWithGiver> hasGiven { get; set; }

        /// <summary>
        /// 处刑对象
        /// </summary>
        Pawn victim { get; }
    }

    public interface IWithGiver
    {
        /// <summary>
        /// 本对象的提供者或者维护者
        /// </summary>
        Thing Giver { get; set; }

        /// <summary>
        /// 作为接口的提供者或者维护者（其应当实现了ITortureThing这个接口）
        /// </summary>
        ITortureThing GiverAsInterface { get; set; }
    }
}