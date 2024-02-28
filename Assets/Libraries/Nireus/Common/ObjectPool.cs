using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Nireus
{
    public class DictionaryPool<TKey, TValue>
    {
        private static readonly ObjectPoolImpl<Dictionary<TKey, TValue>> _dictionaryPool = new ObjectPoolImpl<Dictionary<TKey, TValue>>(null, l => l.Clear());

        public static Dictionary<TKey, TValue> Get()
        {
            return _dictionaryPool.Get();
        }

        public static void Release(Dictionary<TKey, TValue> list)
        {
            _dictionaryPool.Release(list);
        }
    }
    public class ListPool<T>
    {
        private static readonly ObjectPoolImpl<List<T>> _listPool = new ObjectPoolImpl<List<T>>(null, l => l.Clear());

        public static List<T> Get()
        {
            return _listPool.Get();
        }

        public static void Release(List<T> list)
        {
            _listPool.Release(list);
        }
    }
    public class LinkedListPool<T>
    {
        private static readonly ObjectPoolImpl<LinkedList<T>> _listPool = new ObjectPoolImpl<LinkedList<T>>(null, l => l.Clear());

        public static LinkedList<T> Get()
        {
            return _listPool.Get();
        }

        public static void Release(LinkedList<T> list)
        {
            _listPool.Release(list);
        }
    }

    public class ObjectPool<T> where T : new()
    {
        private static readonly Stack<T> _stack = new Stack<T>();
        public static T Get()
        {
            T ret;
            if (_stack.Count == 0)
            {
                ret = new T();
                //countAll++;
            }
            else
            {
                ret = _stack.Pop();
            }
            /*if (_actionOnGet != null)
            {
                _actionOnGet(ret);
            }*/
            return ret;
        }

        public static void Release(T element)
        {
            if (element == null) return;
            if (_stack.Count > 0 && ReferenceEquals(_stack.Peek(), element))
            {
                GameDebug.LogError("ObjectPool Release: element already release to pool");
            }
            /*if (_actionOnRelease != null)
            {
                _actionOnRelease(element);
            }*/
            _stack.Push(element);
        }
        public static void Release(List<T> element_list)
        {
            int len = element_list.Count;
            for(int i = 0; i < len; ++i)
            {
                Release(element_list[i]);
            }
        }
    }

    public class ObjectPoolImpl<T> where T:new()
    {
        private readonly Stack<T> _stack = new Stack<T>();
        private readonly UnityAction<T> _actionOnNew;
        private readonly UnityAction<T> _actionOnGet;
        private readonly UnityAction<T> _actionOnRelease;

        public int countAll { get; private set; }
        public int countActive { get { return countAll - countInactive; } }
        public int countInactive { get { return _stack.Count; } }

        public ObjectPoolImpl(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease,UnityAction<T> actionOnNew = null)
        {
            _actionOnNew = actionOnNew;
            _actionOnGet = actionOnGet;
            _actionOnRelease = actionOnRelease;
        }

        public T Get()
        {
            T ret;
            if(_stack.Count == 0)
            {
                ret = new T();
                if (_actionOnNew != null)
                {
                    _actionOnNew(ret);
                }
                countAll++;
            }
            else
            {
                ret = _stack.Pop();
            }
            if(_actionOnGet != null)
            {
                _actionOnGet(ret);
            }
            return ret;
        }

        public void Release(T element)
        {
            if(_stack.Count > 0 && ReferenceEquals(_stack.Peek(), element))
            {
                GameDebug.LogError("ObjectPool Release: element already release to pool");
            }
            if (_actionOnRelease != null)
            {
                _actionOnRelease(element);
            }
            _stack.Push(element);
        }
    }
}
