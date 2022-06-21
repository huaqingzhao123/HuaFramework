using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.Architecture
{
    public interface IQuery<TResult> : ICanSetArchitecture, IBelongToArchitecture, ICanGetModel, ICanGetSystem, ICanSendQuery
    {
        TResult Do();
    }

    public abstract class AbstractQuery<TResult> : IQuery<TResult>
    {
        public TResult Do()
        {
            return OnDo();
        }

        protected abstract TResult OnDo();

        private IArchitecture _architecture;
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
