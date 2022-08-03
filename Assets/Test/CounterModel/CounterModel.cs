using Assets.Test.CounterModelTest;
using HuaFramework.Architecture;
using HuaFramework.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterModel : AbstractModel,ICountModel
{

    protected override void OnInit()
    {
        var storage = this.GetUtility<IStorage>();
        Count.Value = storage.GetInt("COUNTER_COUNT", 0);
        Count.Register (count =>
        {
            Debug.LogError("count的值为:" + count);
            storage.SaveInt("COUNTER_COUNT", count);
        });
    }

    public BindableProperty<int> Count { get; } = new BindableProperty<int>() { Value = 0 };

}
 public interface ICountModel:IModel
{
    BindableProperty<int> Count { get; }
}
