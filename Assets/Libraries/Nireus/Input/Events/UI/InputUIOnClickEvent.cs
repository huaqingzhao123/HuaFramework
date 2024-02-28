using UnityEngine;
using System.Collections;

public class InputUIOnClickEvent : InputUIEventBase
{
    public InputUIOnClickEvent() : base()
    {
        m_type = InputUIEventType.Click;
    }

    public InputUIOnClickEvent(string UIName, string ComponentName, string pram = null)
        : base(UIName, ComponentName, InputUIEventType.Click, pram)
    {
    }

    public static string GetEventKey(string UIName, string ComponentName, string pram = null)
    {
        return UIName + "." + ComponentName + "." + pram + "." + InputUIEventType.Click.ToString();
    }
}
public class InputUIOnElementClickEvent : InputUIEventBase
{
    public GameObject go;
    public InputUIOnElementClickEvent() : base()
    {
        m_type = InputUIEventType.Click;
    }

    public InputUIOnElementClickEvent(string UIName, string ComponentName, string pram = null)
        : base(UIName, ComponentName, InputUIEventType.Click, pram)
    {
    }

    public static string GetEventKey(string UIName, string ComponentName, string pram = null)
    {
        return UIName + "." + ComponentName + "." + pram + "." + InputUIEventType.Click.ToString();
    }
}