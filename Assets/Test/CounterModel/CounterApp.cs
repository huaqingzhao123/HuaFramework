using HuaFramework.Architecture;
using UnityEngine;

namespace Assets.Test.CounterModelTest
{
    public class CounterApp : Architecture<CounterApp>
    {
        //第一get或者register时调用
        protected override void Init()
        {
            RegisterUtility<IStorage>(new PlayStorage());
            RegisterModel<ICountModel>(new CounterModel());
        }
    }
}
