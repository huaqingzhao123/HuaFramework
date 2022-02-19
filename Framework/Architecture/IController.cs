using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HuaFramework.Architecture
{
    /// <summary>
    /// architecture阉割需在子类实现
    /// </summary>
    public interface IController:IBelongToArchitecture,ICanSendCommond,ICanGetModel,ICanGetSystem,ICanRegisterEvent, ICanSendQuery
    {
        void Init();
    }

}
