using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace COF_Torture.Data
{
    public static class ModLog
    {
        public static void Error(string message) => Log.Error("[" + SaveStorage.ModId + "] " + message);

        /// <summary>
        /// 游戏开始时的各种必定显示的提示
        /// </summary>
        /// <param name="message"></param>
        public static void Message_Start(string message) => Log.Message("[" + SaveStorage.ModId + "] " + message);

        /// <summary>
        /// 只在开发者模式显示
        /// </summary>
        /// <param name="message"></param>
        public static void Message(string message)
        {
            if (Prefs.DevMode)
                Log.Message("[" + SaveStorage.ModId + "] " + message);
        }

        public static void Warning(string message) => Log.Warning("[" + SaveStorage.ModId + "] " + message);

        public static void MessageEach<T>(IEnumerable<T> objects, Func<T, string> func)
        {
            foreach (var o in objects)
            {
                Message_Start(func(o));
            }
        }
    }
}