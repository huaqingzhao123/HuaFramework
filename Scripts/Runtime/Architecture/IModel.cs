using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.Architecture
{
    public interface IModel:IBelongToArchitecture,ICanSetArchitecture, ICanGetUtility,ICanSendEvent
    {
        void Init();
    }
    public abstract class AbstractModel : IModel
    {
        private IArchitecture _architecture;

        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return _architecture;
        }

        void IModel.Init()
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
