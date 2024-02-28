
using System;

namespace XMLib
{
    /// <summary>
    /// EventDispatcherExtend
    /// </summary>
    public static class EventDispatcherExtension
    {
        public static EventHandler On(this EventDispatcher dispatcher, int eventType, Action callback, object group = null, bool matchingParams = false)
        {
            return dispatcher.On(eventType, callback.Target, callback.Method, group, matchingParams);
        }

        public static EventHandler On<T>(this EventDispatcher dispatcher, int eventType, Action<T> callback, object group = null, bool matchingParams = false)
        {
            return dispatcher.On(eventType, callback.Target, callback.Method, group, matchingParams);
        }

        public static EventHandler On<T1, T2>(this EventDispatcher dispatcher, int eventType, Action<T1, T2> callback, object group = null, bool matchingParams = false)
        {
            return dispatcher.On(eventType, callback.Target, callback.Method, group, matchingParams);
        }

        public static EventHandler On<T1, T2, T3>(this EventDispatcher dispatcher, int eventType, Action<T1, T2, T3> callback, object group = null, bool matchingParams = false)
        {
            return dispatcher.On(eventType, callback.Target, callback.Method, group, matchingParams);
        }

        public static EventHandler On<T1, T2, T3, T4>(this EventDispatcher dispatcher, int eventType, Action<T1, T2, T3, T4> callback, object group = null, bool matchingParams = false)
        {
            return dispatcher.On(eventType, callback.Target, callback.Method, group, matchingParams);
        }
    }
}