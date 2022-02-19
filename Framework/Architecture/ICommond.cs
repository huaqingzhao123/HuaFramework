using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.Architecture
{
    public interface ICommond : IBelongToArchitecture, ICanSetArchitecture,ICanGetUtility,ICanGetModel,ICanGetSystem,ICanSendEvent,ICanSendCommond, ICanSendQuery
    {
        void Excute();
        //void Excute(object obj);
        //void Excute(params object[] obj);
    }
    public abstract class AbstractCommond : ICommond
    {
        private IArchitecture _architecture;
        void ICommond.Excute()
        {
            OnExcute();
        }
        //void ICommond.Excute(object obj)
        //{
        //    OnExcute(obj);
        //}

        //void ICommond.Excute(params object[] obj)
        //{
        //    OnExcute(obj);
        //}
        protected abstract void OnExcute();
        //protected abstract void OnExcute(object obj);
        //protected abstract void OnExcute(params object[] obj);
        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return _architecture;
        }

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture)
        {
            _architecture = architecture;
        }
    }

}

