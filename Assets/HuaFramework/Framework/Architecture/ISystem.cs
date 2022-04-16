using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.Architecture {
    public interface ISystem : IBelongToArchitecture, ICanSetArchitecture,ICanGetUtility,ICanGetModel,ICanRegisterEvent,ICanSendEvent,ICanGetSystem
    {
        void Init();
    }
    public abstract class AbstractSystem : ISystem
    {
        private IArchitecture _architecture;
        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return _architecture;
        }

        void ISystem.Init()
        {
            OnInit();
        }
        protected abstract void OnInit();

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture)
        {
            _architecture = architecture;
        }
    }
}

