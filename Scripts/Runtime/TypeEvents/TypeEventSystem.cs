using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.TypeEvents
{
    public interface ITypeEventSystem
    {
        void Send<T>() where T : new();
        void Send<T>(T e);
        IUnRegister Register<T>(Action<T> onEvent);
        void UnRegister<T>(Action<T> action);

    }
    public interface IUnRegister
    {

        void UnRegister();
    }

    public class UnRegisterOnDestroyTrigger : MonoBehaviour
    {
        private HashSet<IUnRegister> _unRegisters = new HashSet<IUnRegister>();
        public void AddUnRegister(IUnRegister unRegister)
        {
            _unRegisters.Add(unRegister);
        }

        private void OnDestroy()
        {
            foreach (var unRegister in _unRegisters)
            {
                unRegister.UnRegister();
            }
            _unRegisters.Clear();
        }
    }
    public class UnRegisterOnDisableTrigger : MonoBehaviour
    {
        private HashSet<IUnRegister> _unRegisters = new HashSet<IUnRegister>();
        public void AddUnRegister(IUnRegister unRegister)
        {
            _unRegisters.Add(unRegister);
        }

        private void OnDisable()
        {
            foreach (var unRegister in _unRegisters)
            {
                unRegister.UnRegister();
            }
            _unRegisters.Clear();
        }
    }

    public static class UnRegisterExtension
    {
        public static void UnRegisterWhenDestroy(this IUnRegister unRegister, GameObject gameObject)
        {
            var trigger = gameObject.GetComponent<UnRegisterOnDestroyTrigger>();
            if (trigger == null)
                trigger = gameObject.AddComponent<UnRegisterOnDestroyTrigger>();
            trigger.AddUnRegister(unRegister);
        }
        public static void UnRegisterWhenDisable(this IUnRegister unRegister, GameObject gameObject)
        {
            var trigger = gameObject.GetComponent<UnRegisterOnDisableTrigger>();
            if (trigger == null)
                trigger = gameObject.AddComponent<UnRegisterOnDisableTrigger>();
            trigger.AddUnRegister(unRegister);
        }
    }
    public struct TypeEventSystemUnRegister<T> : IUnRegister
    {
        public ITypeEventSystem TypeEventSystem;
        public Action<T> OnEvent;

        public void UnRegister()
        {
            TypeEventSystem.UnRegister<T>(OnEvent);
            TypeEventSystem = null;
            OnEvent = null;
        }
    }

    public class TypeEventSystem : ITypeEventSystem
    {
        public interface IRegistrations
        {

        }
        public static readonly TypeEventSystem Global = new TypeEventSystem();
        public class Registrations<T> : IRegistrations
        {
            public Action<T> OnEvent = (e) => { };
        }

        private Dictionary<Type, IRegistrations> _registrations = new Dictionary<Type, IRegistrations>();
        public IUnRegister Register<T>(Action<T> onEvent)
        {
            var type = typeof(T);
            IRegistrations registrations;
            if (_registrations.TryGetValue(type, out registrations))
            {

            }
            else
            {
                registrations = new Registrations<T>();
                _registrations.Add(type, registrations);
            }
            (registrations as Registrations<T>).OnEvent += onEvent;
            return new TypeEventSystemUnRegister<T>()
            {
                OnEvent = onEvent,
                TypeEventSystem = this
            };
        }

        public void Send<T>() where T : new()
        {
            var e = new T();
            Send<T>(e);
        }

        public void Send<T>(T e)
        {
            var type = typeof(T);
            IRegistrations registrations;
            if (_registrations.TryGetValue(type, out registrations))
            {
                (registrations as Registrations<T>).OnEvent(e);
            }
        }

        public void UnRegister<T>(Action<T> onEvent)
        {
            var type = typeof(T);
            IRegistrations registrations;
            if (_registrations.TryGetValue(type, out registrations))
            {
                (registrations as Registrations<T>).OnEvent -= onEvent;
            }

        }
    }


    public interface IOnEvent<T>
    {
        void OnEvent(T e);
    }

    public static class OnGlobalEventExtension
    {
        public static IUnRegister RegisterEvent<T>(this IOnEvent<T> self)where T : struct
        {
            return TypeEventSystem.Global.Register<T>(self.OnEvent);
        }

        public static void UnRegisterEvent<T>(this IOnEvent<T> self)where T : struct
        {
            TypeEventSystem.Global.UnRegister<T>(self.OnEvent);
        }
    }
}

