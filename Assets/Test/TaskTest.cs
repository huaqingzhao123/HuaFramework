using HuaFramework.TypeEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public struct EventTaskStart { }
public struct EventTaskEnd{ }
public class TaskTest 
{
  private  TypeEventSystem _typeEventSystem;
    public TaskTest(TypeEventSystem typeEventSystem)
    {
        _typeEventSystem = typeEventSystem;
    }
    public async Task Test()
    {
        _typeEventSystem.Send<EventTaskStart>();
        await Task.Delay(TimeSpan.FromSeconds(3));
        _typeEventSystem.Send<EventTaskEnd>();
    }
}
