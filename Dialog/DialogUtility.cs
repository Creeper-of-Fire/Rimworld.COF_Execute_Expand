using System.Collections.Generic;
using System.Linq;
using COF_Torture.Data;
using COF_Torture.Dialog.Units;
using COF_Torture.Utility;
using UnityEngine;
using Verse;

namespace COF_Torture.Dialog
{
    public interface IDialogAssembly
    {
        /// <summary>
        /// 绘制自己
        /// </summary>
        void Draw(Rect rect);
        float width { get; }
        float height { get; }
    }
    public static class DialogUtility
    {
        public static IEnumerable<IWithThingGiver> AllTortureHediff(this Pawn pawn)
        {
            foreach (var h in pawn.health.hediffSet.hediffs)
            {
                if (h is IWithThingGiver hg && hg.Giver != null)
                {
                    yield return hg;
                }
            }
        }

        public class DoubleKeyDictionary<Key1, Key2, Value>
        {
            public Dictionary<Key1, Dictionary<Key2, Value>> multiDict =
                new Dictionary<Key1, Dictionary<Key2, Value>>();

            public void SetSet(Key1 key1, Key2 key2, Value value)
            {
                if (multiDict.ContainsKey(key1))
                {
                    var dict2 = multiDict[key1];
                    if (dict2.ContainsKey(key2))
                        dict2[key2] = value;
                    else
                        dict2.Add(key2, value);
                }
                else
                {
                    var dict2 = new Dictionary<Key2, Value>();
                    dict2.Add(key2, value);
                    multiDict.Add(key1, dict2);
                }
            }

            public void SetAdd(Key1 key1, Key2 key2, Value value)
            {
                if (multiDict.ContainsKey(key1))
                {
                    var dict2 = multiDict[key1];
                    dict2.Add(key2, value);
                }
                else
                {
                    var dict2 = new Dictionary<Key2, Value>();
                    dict2.Add(key2, value);
                    multiDict.Add(key1, dict2);
                }
            }

            public void AddAdd(Key1 key1, Key2 key2, Value value)
            {
                var dict2 = new Dictionary<Key2, Value>();
                dict2.Add(key2, value);
                multiDict.Add(key1, dict2);
            }

            public Value Get(Key1 key1, Key2 key2)
            {
                Value value = default;
                if (key1 != null)
                    if (key2 != null)
                        value = multiDict[key1][key2];
                return value;
            }

            public int Count => multiDict.Count;
        }
        
        /// <summary>
        /// 把多个Gizmo替换成多个按钮
        /// </summary>
        /// <param name="gizmos">含有Gizmo的可枚举对象</param>
        /// <returns>多个按钮</returns>
        public static IEnumerable<ButtonTextUnit> getButtonInfo(IEnumerable<Command> gizmos)
        {
            return gizmos.Select(getButtonInfo).Where(button => button != null);
        }

        /// <summary>
        /// 把单个Gizmo替换成单个按钮信息
        /// </summary>
        /// <param name="gizmo">Gizmo</param>
        /// <returns>按钮</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public static ButtonTextUnit getButtonInfo(Command gizmo)
        {
            ButtonTextUnit button;
            switch (gizmo)
            {
                case Command_Action ca:
                    button = new ButtonTextUnit();
                    button.InitInfo(ca);
                    return button;
                case Command_Toggle ct:
                    button = new ButtonTextUnit();
                    button.InitInfo(ct);
                    return button;
            }

            return null;
        }

        public static void CalcWidth<T>(T menuColumn, ref float maxWidth, GameFont font = GameFont.Medium)
        {
            Text.Font = font;
            //maxHeight = Mathf.Max(maxHeight, Text.CalcHeight(GetLabel(menuColumn), width));
            maxWidth = Mathf.Max(maxWidth, Text.CalcSize(GetLabel(menuColumn)).x);
            Text.Font = GameFont.Medium;
        }

        public static void CalcHeight<T>(T menuColumn, ref float maxHeight, GameFont font = GameFont.Medium)
        {
            Text.Font = font;
            maxHeight = Mathf.Max(maxHeight, Text.CalcSize(GetLabel(menuColumn)).y);
            Text.Font = GameFont.Medium;
        }

        /// <summary>
        /// 获得标签（Label值），主要是用于不检查类型的对象
        /// </summary>
        /// <param name="menuColumn">无类型对象</param>
        /// <returns>Label</returns>
        public static string GetLabel<T>(T menuColumn)
        {
            if (menuColumn is Thing thingColumn)
                return GetLabel<T>(thingColumn);
            if (menuColumn is Hediff hediffColumn)
                return GetLabel<T>(hediffColumn);
            if (menuColumn is DialogUnit unitColumn)
                return GetLabel<T>(unitColumn);
            if (menuColumn is string stringColumn)
                return GetLabel<T>(stringColumn);
            if (menuColumn is TaggedString tagStringColumn)
                return GetLabel<T>(tagStringColumn);
            ModLog.Error(menuColumn + "中没有发现对应的Label");
            return "";
        }

        public static string GetLabel<T>(Thing menuColumn) => menuColumn.Label;
        public static string GetLabel<T>(Hediff menuColumn) => menuColumn.Label;
        public static string GetLabel<T>(DialogUnit menuColumn) => menuColumn.label;
        public static string GetLabel<T>(string menuColumn) => menuColumn;
        public static string GetLabel<T>(TaggedString menuColumn) => menuColumn.ToString();
    }
}