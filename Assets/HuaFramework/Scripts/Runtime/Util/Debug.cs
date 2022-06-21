using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace HuaFramework
{

    public class Debug
    {

        public static void Log(object message)
        {
#if !HuaFrameWorkRealease
            UnityEngine.Debug.Log(message);
#endif
        }
        public static void Log(object message,Color color)
        {
#if !HuaFrameWorkRealease
            UnityEngine.Debug.Log(string.Format("<color={0}>{1}</color>", color, message)) ;
#endif
        }
        public static void LogFormat(string message, params object[] args)
        {
#if !HuaFrameWorkRealease
            UnityEngine.Debug.LogFormat(message,args);
#endif
        }
        public static void LogWarning(object message)
        {
#if !HuaFrameWorkRealease
            UnityEngine.Debug.LogWarning(message);
#endif
        }
        public static void LogError(object message)
        {
#if !HuaFrameWorkRealease
            UnityEngine.Debug.LogError(message);
#endif
        }
        public static void LogErrorFormat(string message,params object[] objects)
        {
#if !HuaFrameWorkRealease
            UnityEngine.Debug.LogErrorFormat(message, objects);
#endif
        }
    }

}

