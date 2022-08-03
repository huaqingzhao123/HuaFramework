using Assets.Test.CounterModelTest;
using HuaFramework.Architecture;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterTest : MonoBehaviour, IController
{
    public IArchitecture  GetArchitecture()
    {
        return CounterApp.Instance;
    }

    public void Init()
    {
        PlayerPrefs.DeleteAll();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var countmodle = GetArchitecture().GetModel<ICountModel>();
            countmodle.Count.Value++;
        }
        if (Input.GetMouseButtonDown(1))
        {
            var countmodle = GetArchitecture().GetModel<ICountModel>();
            countmodle.Count.Value--;
        }
    }
}
