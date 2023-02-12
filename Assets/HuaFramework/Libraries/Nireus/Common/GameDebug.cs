using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Nireus
{
    public static class GameDebug
    {
        [Conditional("ENABLE_DEBUG_LOG"), Conditional("UNITY_EDITOR")]
        public static void Log(object message)
        {
            Debug.Log(message);
        }
        [Conditional("ENABLE_DEBUG_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogError(object message)
        {
            Debug.LogError(message);
        }
        [Conditional("ENABLE_DEBUG_LOG"), Conditional("UNITY_EDITOR")]
        public static void Log(object message, Object context)
        {
            Debug.Log(message, context);
        }
        [Conditional("ENABLE_DEBUG_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogError(object message, Object context)
        {
            Debug.LogError(message,context);
        }
        [Conditional("ENABLE_DEBUG_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message, Object context)
        {
            Debug.LogWarning(message, context);
        }
        [Conditional("ENABLE_DEBUG_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message)
        {
            Debug.LogWarning(message);
        }
        [Conditional("ENABLE_DEBUG_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogErrorFormat(string format, params object[] args)
        {
            Debug.LogErrorFormat(format, args);
        }
        [Conditional("ENABLE_DEBUG_LOG"), Conditional("UNITY_EDITOR")]
        public static void LogFormat(string format, params object[] args)
        {
            Debug.LogFormat(format, args);
        }
    }
}
