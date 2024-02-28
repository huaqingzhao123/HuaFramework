using Nireus;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.UI.InputField;

public class InputUIEventProxy : IInputProxyBase
{
    #region 添加监听
    public static InputElementClickRegisterInfo GetOnElementClickListener(UIElementClickEvent element, string UIName, string ComponentName, string parm, InputEventHandle<InputUIOnElementClickEvent> callback)
    {
        InputElementClickRegisterInfo info = HeapObjectPool<InputElementClickRegisterInfo>.GetObject();
        info.go = element.gameObject;
        info.eventKey = InputUIOnClickEvent.GetEventKey(UIName, ComponentName, parm);
        info.callBack = callback;
        info.m_element = element;
        info.m_OnClick = (go) =>
        {
            DispatchOnElementClickEvent(UIName, ComponentName, parm);
        };

        return info;
    }
    public static InputButtonClickRegisterInfo GetOnClickListener(Button button, string UIName, string ComponentName, string parm, InputEventHandle<InputUIOnClickEvent> callback)
    {
        InputButtonClickRegisterInfo info = HeapObjectPool<InputButtonClickRegisterInfo>.GetObject();

        info.eventKey = InputUIOnClickEvent.GetEventKey(UIName, ComponentName, parm);
        info.callBack = callback;
        info.m_button = button;
        info.m_OnClick = () =>
        {
            DispatchOnClickEvent(UIName, ComponentName, parm);
        };

        return info;
    }
    public static InputEndEditRegisterInfo GetEndEditListener(InputField input, string UIName, string ComponentName, string parm, InputEventHandle<InputUIOnEndEditEvent> callback)
    {
        InputEndEditRegisterInfo info = HeapObjectPool<InputEndEditRegisterInfo>.GetObject();

        info.eventKey = InputUIOnEndEditEvent.GetEventKey(UIName, ComponentName, parm);
        info.callBack = callback;
        info.m_input = input;
        info.m_OnEndEdit = (param) =>
        {
            DispatchOnEndEditEvent(UIName, ComponentName, param);
        };

        return info;
    }
    public static InputValueChangedRegisterInfo GetValueChangedListener(InputField input, string UIName, string ComponentName, string parm, InputEventHandle<InputUIOnValueChangedEvent> callback)
    {
        InputValueChangedRegisterInfo info = HeapObjectPool<InputValueChangedRegisterInfo>.GetObject();

        info.eventKey = InputUIOnValueChangedEvent.GetEventKey(UIName, ComponentName, parm);
        info.callBack = callback;
        info.m_input = input;
        info.m_OnValueChange = (param) =>
        {
            DispatchOnValueChangedtEvent(UIName, ComponentName, param);
        };
        return info;
    }
    public static InputDropValueChangedRegisterInfo GetDropValueChangedListener(Dropdown dropDown, string UIName, string ComponentName, string parm, InputEventHandle<InputUIOnDropValueChangedEvent> callback)
    {
        InputDropValueChangedRegisterInfo info = HeapObjectPool<InputDropValueChangedRegisterInfo>.GetObject();

        info.eventKey = InputUIOnDropValueChangedEvent.GetEventKey(UIName, ComponentName, parm);
        info.callBack = callback;
        info.m_dropdown = dropDown;
        info.m_OnValueChange = (param) =>
        {
            DispatchOnDropValueChangedtEvent(UIName, ComponentName, param.ToString());
        };
        return info;
    }

    public static InputSliderValueChangedRegisterInfo GetSliderValueChangedListener(Slider slider, string UIName, string ComponentName, string parm, InputEventHandle<InputUIOnDropValueChangedEvent> callback)
    {
        InputSliderValueChangedRegisterInfo info = HeapObjectPool<InputSliderValueChangedRegisterInfo>.GetObject();

        info.eventKey = InputUIOnDropValueChangedEvent.GetEventKey(UIName, ComponentName, parm);
        info.callBack = callback;
        info.m_slider = slider;
        info.m_OnValueChange = (param) =>
        {
            DispatchOnDropValueChangedtEvent(UIName, ComponentName, param.ToString());
        };
        return info;
    }
    #endregion
    #region 事件派发
    public static void DispatchOnElementClickEvent(string UIName, string ComponentName, string parm)
    {
        //只有允许输入时才派发事件
        if (IsActive)
        {
            InputUIOnElementClickEvent e = GetUIEvent<InputUIOnElementClickEvent>(UIName, ComponentName, parm);
            InputManagerV2.Dispatch("InputUIOnElementClickEvent", e);
        }
    }
    public static void DispatchOnClickEvent(string UIName, string ComponentName, string parm)
    {
        //只有允许输入时才派发事件
        if (IsActive)
        {
            InputUIOnClickEvent e = GetUIEvent<InputUIOnClickEvent>(UIName, ComponentName, parm);
            InputManagerV2.Dispatch("InputUIOnClickEvent", e);
        }
    }
    public static void DispatchOnEndEditEvent(string UIName, string ComponentName, string parm)
    {
        //只有允许输入时才派发事件
        if (IsActive)
        {
            InputUIOnEndEditEvent e = GetUIEvent<InputUIOnEndEditEvent>(UIName, ComponentName, parm);
            InputManagerV2.Dispatch("InputUIOnEndEditEvent", e);
        }
    }
    public static void DispatchOnValueChangedtEvent(string UIName, string ComponentName, string parm)
    {
        //只有允许输入时才派发事件
        if (IsActive)
        {
            InputUIOnValueChangedEvent e = GetUIEvent<InputUIOnValueChangedEvent>(UIName, ComponentName, parm);
            InputManagerV2.Dispatch("InputUIOnValueChangedEvent", e);
        }
    }
    public static void DispatchOnDropValueChangedtEvent(string UIName, string ComponentName, string parm)
    {
        //只有允许输入时才派发事件
        if (IsActive)
        {
            InputUIOnDropValueChangedEvent e = GetUIEvent<InputUIOnDropValueChangedEvent>(UIName, ComponentName, parm);
            InputManagerV2.Dispatch("InputUIOnDropValueChangedEvent", e);
        }
    }

    #endregion

    #region 事件池
    static T GetUIEvent<T>(string UIName, string ComponentName, string parm) where T:InputUIEventBase,new()
    {
        T msg = HeapObjectPool<T>.GetObject();
        msg.Reset();
        msg.m_name = UIName;
        msg.m_compName = ComponentName;
        msg.m_pram = parm;

        return msg;
    }
    #endregion
}
//public class InputImageClickRegisterInfo : InputEventRegisterInfo<InputUIOnClickEvent>
//{
//    public Image m_image;
//    public UnityAction m_OnClick;

//    public override void RemoveListener()
//    {
//        base.RemoveListener();

//        m_image.onClick.RemoveListener(m_OnClick);
//    }

//    public override void AddListener()
//    {
//        base.AddListener();

//        m_image.onClick.AddListener(m_OnClick);
//    }
//}
public class InputElementClickRegisterInfo : InputEventRegisterInfo<InputUIOnElementClickEvent>
{
    public GameObject go;
    public Nireus.UIElementClickEvent m_element;
    public Action<GameObject> m_OnClick;

    public override void RemoveListener()
    {
        base.RemoveListener();
        go = null;
        m_element.SetClickCallback(null);
    }

    public override void AddListener()
    {
        base.AddListener();

        m_element.SetClickCallback(m_OnClick);
        //m_button.onClick.AddListener(m_OnClick);
    }
}
public class InputButtonClickRegisterInfo : InputEventRegisterInfo<InputUIOnClickEvent>
{
    public Button m_button;
    public UnityAction m_OnClick;

    public override void RemoveListener()
    {
        base.RemoveListener();

        m_button.onClick.RemoveListener(m_OnClick);
    }

    public override void AddListener()
    {
        base.AddListener();

        m_button.onClick.AddListener(m_OnClick);
    }
}
public class InputEndEditRegisterInfo : InputEventRegisterInfo<InputUIOnEndEditEvent>
{
    public InputField m_input;
    public UnityAction<string> m_OnEndEdit;

    public override void AddListener()
    {
        base.AddListener();
        m_input.onEndEdit.AddListener(m_OnEndEdit); 
    }

    public override void RemoveListener()
    {
        base.RemoveListener();
        m_input.onEndEdit.RemoveListener(m_OnEndEdit);
    }
}
public class InputValueChangedRegisterInfo : InputEventRegisterInfo<InputUIOnValueChangedEvent>
{
    public InputField m_input;
    public UnityAction<string> m_OnValueChange;

    public override void AddListener()
    {
        base.AddListener();
        m_input.onValueChanged.AddListener(m_OnValueChange);
    }

    public override void RemoveListener()
    {
        base.RemoveListener();
        m_input.onValueChanged.RemoveListener(m_OnValueChange);
    }
}
public class InputDropValueChangedRegisterInfo : InputEventRegisterInfo<InputUIOnDropValueChangedEvent>
{
    public Dropdown m_dropdown;
    public UnityAction<int> m_OnValueChange;

    public override void AddListener()
    {
        base.AddListener();
        m_dropdown.onValueChanged.AddListener(m_OnValueChange);
    }

    public override void RemoveListener()
    {
        base.RemoveListener();
        m_dropdown.onValueChanged.RemoveListener(m_OnValueChange);
    }
}

public class InputSliderValueChangedRegisterInfo : InputEventRegisterInfo<InputUIOnDropValueChangedEvent>
{
    public Slider m_slider;
    public UnityAction<float> m_OnValueChange;

    public override void AddListener()
    {
        base.AddListener();
        m_slider.onValueChanged.AddListener(m_OnValueChange);
    }

    public override void RemoveListener()
    {
        base.RemoveListener();
        m_slider.onValueChanged.RemoveListener(m_OnValueChange);
    }
}
//public class InputDragRegisterInfo : InputEventRegisterInfo<InputUIOnDragEvent>
//{
//    public DragAcceptor m_acceptor;
//    public InputUIEventDragCallBack m_OnDrag;

//    public override void AddListener()
//    {
//        base.AddListener();
//        m_acceptor.m_OnDrag += m_OnDrag;
//    }

//    public override void RemoveListener()
//    {
//        base.RemoveListener();
//        m_acceptor.m_OnDrag -= m_OnDrag;
//    }
//}
//public class InputBeginDragRegisterInfo : InputEventRegisterInfo<InputUIOnBeginDragEvent>
//{
//    public DragAcceptor m_acceptor;
//    public InputUIEventDragCallBack m_OnBeginDrag;

//    public override void AddListener()
//    {
//        base.AddListener();
//        m_acceptor.m_OnBeginDrag += m_OnBeginDrag;
//    }

//    public override void RemoveListener()
//    {
//        base.RemoveListener();
//        m_acceptor.m_OnBeginDrag -= m_OnBeginDrag;
//    }
//}

//public class InputEndDragRegisterInfo : InputEventRegisterInfo<InputUIOnEndDragEvent>
//{
//    public DragAcceptor m_acceptor;
//    public InputUIEventDragCallBack m_OnEndDrag;

//    public override void AddListener()
//    {
//        base.AddListener();
//        m_acceptor.m_OnEndDrag += m_OnEndDrag;
//    }
//    public override void RemoveListener()
//    {
//        base.RemoveListener();
//        m_acceptor.m_OnEndDrag -= m_OnEndDrag;
//    }
//}