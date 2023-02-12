using UnityEngine;
using System.Collections;

public class InputUIOnEndEditEvent : InputUIEventBase
{
    public InputUIOnEndEditEvent() : base()
    {
        m_type = InputUIEventType.EndEdit;
        //m_pram = "";
    }

    public InputUIOnEndEditEvent(string UIName, string ComponentName, string pram = null)
        : base(UIName, ComponentName, InputUIEventType.EndEdit, pram)
    {
    }
    protected override string GetEventKey()
    {
        return GetEventKey(m_name, m_compName, m_type, "");
    }

    public static string GetEventKey(string UIName, string ComponentName, string pram = null)
    {
        return UIName + "." + ComponentName + "." + "" + "." + InputUIEventType.EndEdit.ToString();
    }
}
public class InputUIOnValueChangedEvent : InputUIEventBase
{
    public InputUIOnValueChangedEvent() : base()
    {
        m_type = InputUIEventType.ValueChanged;
    }

    public InputUIOnValueChangedEvent(string UIName, string ComponentName, string pram = null)
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