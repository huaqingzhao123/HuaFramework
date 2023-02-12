using Nireus;
using UnityEngine;
using UnityEngine.UI;

public static class UGUIEx
{
    #region RectTransform

    public static void SetWidth(this RectTransform rectTransform, float width)
    {
        var size = rectTransform.sizeDelta;
        size.x = width;
        rectTransform.sizeDelta = size;
    }

    public static void SetHeight(this RectTransform rectTransform, float height)
    {
        var size = rectTransform.sizeDelta;
        size.y = height;
        rectTransform.sizeDelta = size;
    }

    public static Canvas GetCanvas(this RectTransform rectTransform)
    {
        if (!rectTransform) return null;
        return rectTransform.GetComponentInParent<Canvas>();
        
        //Transform rt = rectTransform;
        //Canvas canvas = rt.GetComponent<Canvas>();
        //while (canvas == null && rt.parent)
        //{
        //    rt = rt.parent;
        //    canvas = rt.GetComponent<Canvas>();
        //}

        //return canvas;
    }

    public static Camera GetUICamera(this RectTransform rectTransform)
    {
        var canvas = rectTransform.GetCanvas();
        var uiRoot = canvas?.GetComponent<DepthSortUIRoot>();
        return uiRoot?.uiCamera;
    }

    #endregion

    #region ButtonEx
    public static void setTouchEnabled(this Button button, bool enabled)
    {
        button.interactable = enabled;
    }

    public static void SetHighlightToNomalState(this Button button,bool is_highlight)
    {

        Nireus.UIButtonHighlight c = button.GetComponent<Nireus.UIButtonHighlight>();
        if (c == null)
        {
            c = button.gameObject.AddComponent<Nireus.UIButtonHighlight>();
        }
        c.SetHighlightToNomalState(is_highlight);
    }


    #endregion

    #region ImageEx
    public static void setImage(this Image image, Sprite image_sprite)
    {
        image.enabled = image_sprite != null;
        image.sprite = image_sprite;
    }

    public static void SetAlpha(this Image image, float a)
    {
        if (Mathf.Approximately(a, image.color.a) == false)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, a);
        }
    }

    public static void SetAlpha(this Image image, byte byteAlpha)
    {
        if (image.color.a * 255 != byteAlpha)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, byteAlpha);
        }
    }

    public static void setColor(this Image image, float r, float g, float b)
    {
        image.color = new Color(r / 255.0f, g / 255.0f, b / 255.0f);
    }

    // 类似于 FF00FF 字符串;
    public static void setColor(this Image image, string s16)
    {
        float r = System.Convert.ToInt32(s16.Substring(0, 2), 16);
        float g = System.Convert.ToInt32(s16.Substring(2, 2), 16);
        float b = System.Convert.ToInt32(s16.Substring(4, 2), 16);
        image.setColor(r, g, b);
    }

    // 恢复大小到image图片大小;
    public static void resetSize(this Image image)
    {
        image.SetNativeSize();
    }

    public static void onClick(this Image image, System.Action<Image> OnClick)
    {
        System.Action<GameObject> fun = (GameObject go) =>
        {
            if (OnClick != null)
            {
                OnClick(image);
            }
        };
        Nireus.UIElementClickEvent ui_event = image.gameObject.GetComponent<Nireus.UIElementClickEvent>();
        if (ui_event == null)
        {
            ui_event = image.gameObject.AddComponent<Nireus.UIElementClickEvent>();
        }
        //AddOnElementClickListener(ui_event,()=> { },);
        ui_event.SetClickCallback(OnClick == null ? null : fun);
    }
    public static void AddOnElementClickListener(this Image image, InputEventHandle<InputUIOnElementClickEvent> callback,
   string UIEventKey, string inputName, string parm = null)
    {
        Nireus.UIElementClickEvent ui_event = image.gameObject.GetComponent<Nireus.UIElementClickEvent>();
        if (ui_event == null)
        {
            ui_event = image.gameObject.AddComponent<Nireus.UIElementClickEvent>();
        }

        InputElementClickRegisterInfo info =
            InputUIEventProxy.GetOnElementClickListener(ui_event, UIEventKey, inputName, parm, callback);
        info.AddListener();
        //m_OnDropValueChangedEvents.Add(info);
    }
    #endregion

    #region InputFieldEx

    #endregion

    #region TextEx
    public static void setText(this Text ui_text, string text)
    {
        ui_text.text = text.Replace("\\n", "\n");
    }

    public static void setText(this Text ui_text, long number)
    {
        ui_text.text = number.ToString();
    }

    public static void setText(this Text ui_text, float number)
    {
        ui_text.text = number.ToString();
    }

    public static void setTextRich(this Text ui_text, string text)
    {
        if (ui_text.supportRichText == false)
        {
            ui_text.enableRichText(true);
        }
        ui_text.text = text.Replace("\\n", "\n");
    }

    public static void SetAlpha(this Text ui_text, float a)
    {
        if (Mathf.Approximately(a, ui_text.color.a) == false)
        {
            ui_text.color = new Color(ui_text.color.r, ui_text.color.g, ui_text.color.b, a);
        }
    }

    // 是否支持类html文字，默认是不支持;
    public static void enableRichText(this Text ui_text, bool enabled)
    {
        ui_text.supportRichText = enabled;
    }

    public static void isOnNoEvent(this Toggle toggle, bool is_on)
    {
        var value_change = toggle.onValueChanged;
        toggle.onValueChanged = new UnityEngine.UI.Toggle.ToggleEvent();
        toggle.isOn = is_on;
        toggle.onValueChanged = value_change;
    }
    #endregion
}
