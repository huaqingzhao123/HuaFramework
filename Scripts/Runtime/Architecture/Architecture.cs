using HuaFramework.IOC;
using HuaFramework.TypeEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HuaFramework.Architecture
{

    public abstract class Architecture<T> : IArchitecture where T : Architecture<T>, new()
    {
        private static T _architecture;

        public static Action<T> OnRegisterPatch = architecture => { };
        private bool _inited;
        private List<IModel> _models = new List<IModel>();
        private List<ISystem> _systems = new List<ISystem>();
        /// <summary>
        /// 生成单例
        /// </summary>
        static void MakeSureArchitecture()
        {
            if (_architecture == null)
            {
                _architecture = new T();
                //单例初始化
                _architecture.Init();
                OnRegisterPatch?.Invoke(_architecture);
                foreach (var item in _architecture._models)
                {
                    item.Init();
                }
                foreach (var item in _architecture._systems)
                {
                    item.Init();
                }
                _architecture._models.Clear();
                _architecture._systems.Clear();
                _architecture._inited = true;
            }
        }

        public static T Instance
        {   
            get
            {
                if (_architecture == null)
                    MakeSureArchitecture();
                return _architecture;
            }
        }
        public static void Reset()
        {
            _architecture = null;
        }
        /// <summary>
        /// 全局单例初始化完成后调用
        /// </summary>
        protected abstract void Init();

        private IOCContainer _iOCContainner = new IOCContainer();

        //public static T Get<T>() where T : class
        //{
        //    MakeSureArchitecture();
        //    return _architecture._iOCContainner.Get<T>();
        //}
        //public static void Register<T>(T instance)
        //{
        //    MakeSureArchitecture();
        //    _architecture._iOCContainner.Register<T>(instance);
        //}


        #region 内部调用
        public void RegisterModel<T>(T model) where T : IModel
        {
            model.SetArchitecture(this);
            _iOCContainner.Register<T>(model);
            if (!_inited)
            {
                _models.Add(model);
            }
            else
            {
                model.Init();
            }
        }
        public T GetModel<T>() where T : class, IModel
        {
            return _iOCContainner.Get<T>();
        }


        public void RegisterUtility<T>(T utility)
        {
            _iOCContainner.Register(utility);
        }
        public T GetUtility<T>() where T : class, IUtility
        {
            return _iOCContainner.Get<T>();
        }

        public void RegisterSystem<T>(T system) where T : ISystem
        {
            system.SetArchitecture(this);
            _iOCContainner.Register<T>(system);
            if (!_inited)
            {
                _systems.Add(system);
            }
            else
            {
                system.Init();
            }
        }
        public T GetSystem<T>() where T : class, ISystem
        {
            return _iOCContainner.Get<T>();
        }

        public void SendCommond<T>() where T : ICommond,new()
        {
            var commond = new T();
            commond.SetArchitecture(this);
            commond.Excute();
        }
        //public void SendCommond<T>(object obj) where T : ICommond, new()
        //{
        //    var commond = new T();
        //    commond.SetArchitecture(this);
        //    commond.Excute(obj);
        //}
        //public void SendCommond<T>(params object[] obj) where T : ICommond, new()
        //{
        //    var commond = new T();
        //    commond.SetArchitecture(this);
        //    commond.Excute(obj);
        //}
        public void SendCommond<T>(T commond) where T : ICommond
        {
            commond.SetArchitecture(this);
            commond.Excute();
        }

        public TResult SendQuery<TResult>(IQuery<TResult> query) 
        {
            query.SetArchitecture(this);
            return query.Do();
        }
        private ITypeEventSystem _typeEventSystem = new TypeEventSystem();
        public void SendEvent<T>() where T : new()
        {
            _typeEventSystem.Send<T>();
        }

        public void SendEvent<T>(T e)
        {
            _typeEventSystem.Send<T>(e);
        }

        public IUnRegister RegisterEvent<T>(Action<T> onEvent)
        {
           return _typeEventSystem.Register(onEvent);
        }

        public void UnRegisterEvent<T>(Action<T> onEvent)
        {
            _typeEventSystem.UnRegister(onEvent);
        }

        public void Clear()
        {
            _iOCContainner.Clear();
        }

   
        #endregion
    }

}

