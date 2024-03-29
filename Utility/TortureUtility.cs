using System;
using System.Collections.Generic;
using System.Linq;
using COF_Torture.Data;
using COF_Torture.Hediffs;
using COF_Torture.Patch;
using COF_Torture.Things;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using BodyPartTagDefOf = COF_Torture.Utility.DefOf.BodyPartTagDefOf;
using DamageDefOf = COF_Torture.Utility.DefOf.DamageDefOf;
using HediffDefOf = COF_Torture.Utility.DefOf.HediffDefOf;

namespace COF_Torture.Utility
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
        /// <param name="OrgasmReason"></param>
        public static void Orgasm(Pawn pawn, int OrgasmTimes = 1, Sexual_Gratification OrgasmReason = null )
        {
            if (OrgasmTimes < 1)
                OrgasmTimes = 1;
            Hediff HediffOrgasm;
            bool hasOrgasmBefore;
            HediffComp_Disappears comps1 = null;
            if (pawn.health.hediffSet.HasHediff(HediffDefOf.COF_Torture_Orgasm))
            {
                HediffOrgasm =
                    pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.COF_Torture_Orgasm);
                hasOrgasmBefore = true;
            }
            else
            {
                HediffOrgasm = HediffMaker.MakeHediff(HediffDefOf.COF_Torture_Orgasm, pawn);
                hasOrgasmBefore = false;
            }

            for (int i = 0; i < OrgasmTimes; i++)
            {
                if (SettingPatch.RimJobWorldIsActive)
                {
                    Need need = pawn.needs.AllNeeds.Find(x => x.def == SettingPatch.SexNeed);
                    need.CurLevel += satisfySexNeedWhenOrgasm; //因为高潮获得了性满足
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
                    pawn.health.AddHediff(HediffDefOf.COF_Torture_Orgasm);
                    hasOrgasmBefore = true;
                }
            }
        }

        public static List<Pawn> ListAllPlayerPawns(Map map)
        {
            var list = new List<Pawn>();
            foreach (var pawn in map.mapPawns.AllPawns)
            {
                if (!pawn.IsColonist)
                    continue;

                if (pawn.IsColonyMech)
                    list.Add(pawn);

                if (pawn.IsColonyMechPlayerControlled)
                    list.Add(pawn);
                
                if (pawn.IsColonist)
                    list.Add(pawn);
            }
            
            return list.Distinct().ToList();
        }

        public static Corpse MakeCorpse_DifferentKind(this Pawn pawn, Building_Grave assignedGrave, bool inBed,
            float bedRotation)
        {
            if (pawn.holdingOwner != null)
            {
                Log.Warning(
                    "We can't make corpse because the pawn is in a ThingOwner. Remove him from the container first. This should have been already handled before calling this method. holder=" +
                    pawn.ParentHolder);
                return null;
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
            corpseDef.label = "BarbecueCorpseLabel".Translate((NamedArgument)pawnDef.label);
            corpseDef.description = "BarbecueCorpseDesc".Translate((NamedArgument)pawnDef.label);
            //corpseDef.thingCategories.Remove(ThingCategoryDefOf.CorpsesHumanlike);
            //corpseDef.thingCategories.Add(Things.ThingCategoryDefOf.BarbecueCorpsesHumanlike);
            corpseDef.comps.Clear();
            corpseDef.comps.Add(new CompProperties_Forbiddable());
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
            ingestible.tasteThought = DefOf.ThoughtDefOf.AteFineMeal; // ThoughtDefOf.AteHumanlikeMeatAsIngredient;
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
        /// <typeparam name="VList">值的列表的类型</typeparam>
        public static void DictListAdd<T,V,VList>(this Dictionary<T, VList> dict, T key, V value) where VList: ICollection<V>, new()
        {
            if (dict.ContainsKey(key))
            {
                dict[key].Add(value);
            }
            else
            {
                var tempList = new VList { value };
                dict.Add(key, tempList);
            }
        }

        public static bool IsMasochist(this Pawn pawn)
        {
            if (pawn.DestroyedOrNull()) return false;
            if (pawn.story == null) return false;
            if (pawn.story.traits == null) return false;
            return  pawn.story.traits.HasTrait(DefOf.TraitDefOf.Masochist);
        }
        /*public static void DictListAdd<T, V>(this Dictionary<T, List<V>> dict, T key, V value)
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
        }*/

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
            if (SettingPatch.RimJobWorldIsActive && pawn.IsMasochist())
            {
                execute = DamageDefOf.Execute_Licentious;
                dHediff = HediffMaker.MakeHediff(HediffDefOf.COF_Torture_Licentious, pawn);
            }
            else
            {
                execute = DamageDefOf.Execute;
                dHediff = HediffMaker.MakeHediff(HediffDefOf.COF_Torture_Fixed, pawn);
            }

            var dInfo = new DamageInfo(execute, 1);

            bool ShouldBeDeathrestingOrInComaInsteadOfDead(Pawn p)
            {
                if (!ModsConfig.BiotechActive || p.genes == null || !p.genes.HasGene(GeneDefOf.Deathless))
                    return false;
                BodyPartRecord brain = p.health.hediffSet.GetBrain();
                return brain != null && !p.health.hediffSet.PartIsMissing(brain) &&
                       p.health.hediffSet.GetPartHealth(brain) > 0.0;
            } //这里实际上是SanguophageUtility.ShouldBeDeathrestingOrInComaInsteadOfDead，但是因为ShouldBeDead被改过所以只能重写

            if (ShouldBeDeathrestingOrInComaInsteadOfDead(pawn))
            {
                var ForceDeathrestOrComa = AccessTools.Method(typeof(Pawn_HealthTracker), "ForceDeathrestOrComa");
                ForceDeathrestOrComa.Invoke(pawn.health, new[] { dInfo, (object)dHediff });
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
            if (PawnCapacityUtility.CalculatePartEfficiency(health.hediffSet, pawn.RaceProps.body.corePart) <=
                0.0)
            {
                if (DebugViewSettings.logCauseOfDeath)
                    ModLog.Message_Start("CauseOfDeath: zero efficiency of " + pawn.RaceProps.body.corePart.Label);
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
        /// 去除字典中的值
        /// </summary>
        /// <param name="dict">字典</param>
        /// <param name="func">值满足的函数</param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        public static void RemoveAt<T, V>(this Dictionary<T, V> dict, Func<V, bool> func)
        {
            var list = new List<T>();
            foreach (var pair in dict)
            {
                if (func(pair.Value))
                    list.Add(pair.Key);
            }

            foreach (var key in list)
            {
                dict.Remove(key);
            }
        }

        /// <summary>
        /// 对所有值为list的字典的每个Value执行某个动作
        /// </summary>
        /// <param name="dict">字典</param>
        /// <param name="action">动作（无返回值）,参数为V</param>
        /// <typeparam name="T">key类型</typeparam>
        /// <typeparam name="V">list内容的类型</typeparam>
        public static void ForeachDL<T, V>(this Dictionary<T, List<V>> dict, Action<V> action)
        {
            foreach (var list in dict.Values)
            {
                foreach (var value in list)
                {
                    action(value);
                }
            }
        }

        /// <summary>
        /// 对所有值为list的字典的每个Value执行某个动作
        /// </summary>
        /// <param name="dict">字典</param>
        /// <param name="action">动作（无返回值），参数为T</param>
        /// <typeparam name="T">key类型</typeparam>
        /// <typeparam name="V">list内容的类型</typeparam>
        public static void ForeachDL<T, V>(this Dictionary<T, List<V>> dict, Action<T> action)
        {
            foreach (var list in dict)
            {
                foreach (var value in list.Value)
                {
                    action(list.Key);
                }
            }
        }

        /// <summary>
        /// 对所有值为list的字典的每个Value执行某个动作
        /// </summary>
        /// <param name="dict">字典</param>
        /// <param name="action">动作（无返回值），参数为T和V</param>
        /// <typeparam name="T">key类型</typeparam>
        /// <typeparam name="V">list内容的类型</typeparam>
        public static void ForeachDL<T, V>(this Dictionary<T, List<V>> dict, Action<T, V> action)
        {
            foreach (var list in dict)
            {
                foreach (var value in list.Value)
                {
                    action(list.Key, value);
                }
            }
        }

        public static Dictionary<T, List<V>> untieNestedDict<T, V>(Dictionary<T, List<V>> allParts, T defaultT,
            int minGroupLength = 0)
        {
            var fullGroups = new Dictionary<T, List<V>>();
            var regroupedGroups = new Dictionary<T, List<V>>();
            //var unfiledGroups = new List<T>();
            var unfiledParts = new List<V>(); //非引用而是复制
            var otherGroup = new List<V>();
            GetFullGroups();
            GetOtherGroup();
            ReGroup(); //进入递归
            if (unfiledParts.NullOrEmpty())
            {
                foreach (var v in unfiledParts)
                {
                    ModLog.Error("没有分类" + v);
                    //regroupedGroups.DictListAdd(defaultT, v);
                }
            }

            regroupedGroups.Add(defaultT, otherGroup);
            return regroupedGroups;

            void GetFullGroups()
            {
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
            }


            void GetOtherGroup()
            {
                if (minGroupLength <= 0)
                    return;
                foreach (var Group in fullGroups)
                {
                    if (Group.Value.Count < minGroupLength)
                    {
                        foreach (var bodyPart in Group.Value)
                        {
                            otherGroup.Add(bodyPart);
                            unfiledParts.Remove(bodyPart);
                        }
                    }
                }

                otherGroup = otherGroup.Distinct().ToList();
            }

            void ReGroup()
            {
                var minGroup = GetMinBodyPartGroup(fullGroups); //找到要分类的
                //ModLog.Message("" + minGroup);
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
                    if (Group.Value.NullOrEmpty())
                        continue;
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
                else
                    ModLog.Error(value1 + "无法在" + typeof(T1) + "和" + typeof(T2) + "之间进行类型转换，错误地点位于" + list1);
            }

            return list2;
        }

        public static bool IsOneOfThem(this string This, params string[] strS)
        {
            foreach (var str in strS)
            {
                if (str == This)
                    return true;
            }

            return false;
        }

        public static bool ContainOneOfThem(this string This, params string[] strS)
        {
            foreach (var str in strS)
            {
                if (This.Contains(str))
                    return true;
            }

            return false;
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

        string Label { get; }

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

        List<IWithThingGiver> hasGiven { get; set; }

        /// <summary>
        /// 处刑对象
        /// </summary>
        Pawn victim { get; }
    }

    public interface IWithThingGiver
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