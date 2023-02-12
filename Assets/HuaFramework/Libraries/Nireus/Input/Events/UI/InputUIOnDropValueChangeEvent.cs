using UnityEngine;
using System.Collections;

public class InputUIOnDropValueChangedEvent : InputUIEventBase
{
    public InputUIOnDropValueChangedEvent() : base()
    {
        m_type = InputUIEventType.ValueChanged;
    }

    public InputUIOnDropValueChangedEvent(string UIName, string ComponentName, string pram = null)
        : base(UIName, ComponentName, InputUIEventType.ValueChanged, pram)
    {
    }
    protected override string GetEventKey()
    {
        return GetEventKey(m_name, m_compName, m_type, "");
    }
    public static string GetEventKey(string UIName, string ComponentName, string pram = null)
    {
        return UIName + "." + ComponentName + "." + "" + "." + InputUIEventType.ValueChanged.ToString();
    }
}