using System.Collections.Generic;
using System.Linq;
using COF_Torture.Data;
using COF_Torture.Patch;
using COF_Torture.Utility;
using Verse;

namespace COF_Torture.Body
{
    public static class BodyUtility
    {
        /// <summary>
        /// 原本的BodyPartGroup是层层嵌套的，这个算法把嵌套解开，使得一个BodyPart只对应一个BodyPartGroup。/n这个算法同样适用于其他情况
        /// </summary>
        /// <param name="allParts">键：BodyPartGroup，值：其包含的BodyPart列表，彼此重复。</param>
        /// <param name="defaultKey">默认键值</param>
        /// <param name="minGroupLength"></param>
        /// <returns>键：BodyPartGroup，值：其包含的BodyPart列表，互不重复。</returns>
        public static Dictionary<string, List<BodyPartRecord>> untieNestedDict(
            Dictionary<string, List<BodyPartRecord>> allParts, string defaultKey,
            int minGroupLength = 3)
        {
            var fullGroups = new Dictionary<string, List<BodyPartRecord>>();
            var regroupedGroups = new Dictionary<string, List<BodyPartRecord>>();
            var directAddParts = new List<BodyPartRecord>();
            //var regroupedGroups2 = new Dictionary<string, List<BodyPartRecord>>();
            var unfiledParts = new List<BodyPartRecord>(); //非引用而是复制
            var otherGroup = new List<BodyPartRecord>();
            var rjwGroup = new List<BodyPartRecord>();
            const string rjwName = "RJW";
            var unfiledGroups = allParts.Keys.ToList();

            GetFullParts();
            //unfiledParts.Clear();
            //if (SettingPatch.RimJobWorldIsActive)
            //SetDirectGroupRJW();
            SetDirectGroup();
            //GetFullParts();
            ReGroup();
            GetOtherGroup();


            if (SettingPatch.RimJobWorldIsActive && !rjwName.NullOrEmpty())
                regroupedGroups.Add(rjwName, rjwGroup);
            regroupedGroups.Add(defaultKey, otherGroup);

            var outGroups = new Dictionary<string, List<BodyPartRecord>>();
            regroupedGroups.ForeachDL((group, part) => { outGroups.DictListAdd(group, part); });
            outGroups.RemoveAt(list => list.NullOrEmpty());
            outGroups.Remove("");

            return outGroups;

            /*void SetDirectGroupRJW()
            {
                var parts = unfiledParts;
                //var groups = unfiledGroups;
                fullGroups.ForeachDL((group, part) =>
                {
                    if (!parts.Contains(part))
                        return;
                    if (part.IsBodyPartRJW())
                    {
                        rjwGroup.Add(part);
                        parts.Remove(part);
                        //if (rjwName.NullOrEmpty())
                        //    rjwName = part.def.label;
                    }
                });
                unfiledParts = parts.Distinct().ToList();
            }*/

            void SetDirectGroup()
            {
                var parts = unfiledParts;
                //var groups = unfiledGroups;
                fullGroups.ForeachDL((group, part) =>
                {
                    if (!parts.Contains(part))
                        return;
                    if (part.def.label.Contains(group) || group.Contains(part.def.label))
                    {
                        regroupedGroups.DictListAdd(group, part);
                        parts.Remove(part);
                    }
                });
                unfiledParts = parts.Distinct().ToList();
            }

            void GetOtherGroup()
            {
                var groups = regroupedGroups;
                if (minGroupLength <= 0)
                    return;
                foreach (var Group in groups)
                {
                    if (Group.Value.Count >= minGroupLength) continue;
                    foreach (var bodyPart in Group.Value)
                    {
                        if (bodyPart.IsBodyPartRJW())
                        {
                            rjwGroup.Add(bodyPart);
                            //ModLog.Message("添加" + bodyPart);
                        }
                        else
                            otherGroup.Add(bodyPart);

                        unfiledParts.Remove(bodyPart);
                    }
                }

                otherGroup = otherGroup.Distinct().ToList();
                rjwGroup = rjwGroup.Distinct().ToList();

                foreach (var part in otherGroup)
                {
                    foreach (var list in regroupedGroups.Values)
                    {
                        list.Remove(part);
                    }
                }

                foreach (var part in rjwGroup)
                {
                    foreach (var list in regroupedGroups.Values)
                    {
                        list.Remove(part);
                    }
                }
            }


            void ReGroup()
            {
                var minKey = GetMinBodyPartGroup(unfiledGroups); //找到要分类的
                //ModLog.Message("" + minKey);
                if (!fullGroups.ContainsKey(minKey)) return;
                var minGroup = fullGroups[minKey];
                foreach (var bodyPart in minGroup)
                {
                    if (unfiledParts.Contains(bodyPart))
                        //目的是分类bodyPart到Group里面，unfiledParts里面的就是没有分类的bodyPart
                    {
                        unfiledParts.Remove(bodyPart);
                        regroupedGroups.DictListAdd(minKey, bodyPart);
                    }
                }

                unfiledGroups.Remove(minKey);
                if (unfiledGroups.NullOrEmpty())
                    return;
                ReGroup();
            }

            string GetMinBodyPartGroup(
                List<string> Groups)
            {
                var minGroup = "";
                foreach (var key in Groups)
                {
                    if (!fullGroups.ContainsKey(key))
                        continue;
                    var Group = fullGroups[key];
                    if (Groups.NullOrEmpty())
                        continue;
                    if (minGroup == "")
                    {
                        minGroup = key;
                        continue;
                    }

                    if (fullGroups.ContainsKey(minGroup) && Group.Count < fullGroups[minGroup].Count)
                    {
                        minGroup = key;
                    }
                }

                return minGroup;
            }

            void GetFullParts()
            {
                //fullGroups.Clear();
                var parts = unfiledParts;
                var groups = unfiledGroups;
                allParts.ForeachDL((group, part) =>
                {
                    fullGroups.DictListAdd(group, part);
                    parts.Add(part);
                    groups.Add(group);
                });
                unfiledParts = parts.Distinct().ToList();
                unfiledGroups = groups.Distinct().ToList();
                fullGroups.RemoveAt(list => list.NullOrEmpty());
            }
        }

        public static bool IsBodyPartRJW(this BodyPartRecord bodyPart)
        {
            return Enumerable.Any(bodyPart.groups,
                       group => group.defName.ContainOneOfThem("Genitals", "Chest", "Flank", "Anus")) ||
                   bodyPart.def.defName.IsOneOfThem("Genitals", "Chest", "Flank", "Anus");
        }

        /// <summary>
        /// 重要！！！！因为带有hediff的AddHediff被harmonyPatch了，所以不能重复调用
        /// </summary>
        /// <param name="part"></param>
        /// <param name="hediff"></param>
        public static void AddVirtualHediff(this VirtualPartRecord part, Hediff hediff)
        {
            part.AddHediff(hediff);
        }

        /// <summary>
        /// 找到虚拟部件树，随机进行添加，且不对实际部件进行添加
        /// </summary>
        /// <param name="health"></param>
        /// <param name="hediff"></param>
        /// <param name="part"></param>
        public static void AddVirtualHediff(this Pawn_HealthTracker health,
            Hediff hediff, BodyPartRecord part)
        {
            if (part == null) return;
            var pawn = health.hediffSet.pawn;
            var vTree = pawn.GetVirtualPartTree(part);
            vTree?.AddHediff(hediff);
        }

        public static VirtualPartTree GetVirtualPartTree(this Pawn pawn, BodyPartRecord part)
        {
            var virtualParts = pawn.GetPawnData().VirtualParts.VirtualTrees;
            if (virtualParts.ContainsKey(part))
                return virtualParts[part];
            else
                return null;
        }


        public static bool HasVirtualParts(Pawn pawn, BodyPartRecord bodyPartRecord)
        {
            return pawn.GetPawnData().VirtualParts.VirtualTrees.ContainsKey(bodyPartRecord);
        }

        public static Dictionary<BodyPartRecord, VirtualPartTree> GetVirtualPartTrees(this Pawn pawn)
        {
            return pawn.GetPawnData().VirtualParts.VirtualTrees;
        }

        public static IEnumerable<BodyPartRecord> GetVirtualParts(Pawn pawn)
        {
            foreach (var virtualPartTree in pawn.GetPawnData().VirtualParts.VirtualTrees.Values)
            {
                foreach (var virtualPart in virtualPartTree.AllParts)
                {
                    yield return virtualPart;
                }
            }
        }

        public static VirtualPartRecord TryGetVirtualPart(this Hediff hediff)
        {
            var data = hediff.pawn.GetPawnData().VirtualParts;
            foreach (var pair in data.VirtualHediffByPart)
            {
                if (pair.Value.Contains(hediff))
                    return data.GetVirtualPartRecordByUniLabel(pair.Key);
            }

            return null;
        }
    }
}