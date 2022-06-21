using HuaFramework.TypeEvents;
using System;

namespace HuaFramework.Architecture {

    public interface IArchitecture
    {
        void RegisterModel<T>(T model) where T : IModel;
        void RegisterUtility<T>(T utility);
        void RegisterSystem<T>(T system) where T : ISystem;
        T GetUtility<T>() where T : class, IUtility;
        T GetModel<T>() where T : class, IModel;
        T GetSystem<T>() where T : class, ISystem;
        void SendCommond<T>() where T : ICommond,new();
        //void SendCommond<T>(object obj) where T : ICommond, new();
        //void SendCommond<T>(params object[] obj) where T : ICommond, new();
        void SendCommond<T>(T commond) where T : ICommond;
        TResult SendQuery<TResult>(IQuery<TResult> query);
        void SendEvent<T>()where T : new();
        void SendEvent<T>(T e);
        IUnRegister RegisterEvent<T>(Action<T> onEvent);
        void UnRegisterEvent<T>(Action<T> onEvent);

    }

}
