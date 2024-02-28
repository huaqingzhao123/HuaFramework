using System.Collections.Generic;
using System;
using Nireus;
public class InputOperationEventProxy : IInputProxyBase
{
    static List<IInputOperationEventCreater> s_creates = new List<IInputOperationEventCreater>();

    public static void Init()
    {
        //if(!SkillTestRunner.isSkillTest)
        //{
        //    GameRunner.s_OnApplicationUpdate += Update;
        //}
        //else
        //{
        //    SkillTestRunner.s_OnApplicationUpdate += Update;
        //}        
    }

    public static IInputOperationEventCreater LoadEventCreater<T>() where T : IInputOperationEventCreater, new()
    {
        for (int i = 0; i < s_creates.Count; i++)
        {
            if (s_creates[i] is T)
            {
                throw new Exception(typeof(T).Name + " Creater has Exits!");
            }
        }

        IInputOperationEventCreater creater = new T();
        s_creates.Add(creater);

        return creater;
    }

    public static void UnLoadEventCreater<T>() where T : IInputOperationEventCreater , new()
    {
        for (int i = 0; i < s_creates.Count; i++)
        {
            if(s_creates[i] is T)
            {
                s_creates.RemoveAt(i);
            }
        }
    }

    public static void Update()
    {
        if(IsActive)
        {
            for (int i = 0; i < s_creates.Count; i++)
            {
                try
                {
                    s_creates[i].EventTriggerLogic();
                }
                catch(Exception e)
                {
                    GameDebug.LogError(e.ToString());
                }
            }
        }
    }

    public static void DispatchInputOperationEvent(IInputOperationEventBase inputOperationEventBase,string eventName)
    {
        //只有允许输入时才派发事件
        if (IsActive)
        {
            InputManagerV2.Dispatch(eventName, inputOperationEventBase);
        }
    }

}