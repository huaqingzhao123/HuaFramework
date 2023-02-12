using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RedDot : MonoBehaviour
{
    public RedDotType type = RedDotType.NONE;
    public List<RedDotType> observeTypeList;

    private bool _registed = false;
    private void _checkRegist()
    {
        if (_registed == false)
        {
            RedDotManager.getInstance().regist(this);
            _registed = true;
        }
    }

    public void Awake()
    {
        _checkRegist();
    }

    public void show(bool value, bool update = true)
    {
        _checkRegist();
        gameObject.SetActive(value);
        if (update)
        {
            RedDotManager.getInstance().postEvent(this, value);
        }
    }

    public void OnDestroy()
    {
        RedDotManager.getInstance().unregist(this);
    }

#if UNITY_EDITOR
    [ContextMenu("show")]
    private void testShow()
    {
        show(true);
    }
    [ContextMenu("hide")]
    private void testHide()
    {
        show(false);
    }

    [ContextMenu("post event: show")]
    private void testPostShowEvent()
    {
        RedDotManager.getInstance().postEvent(type, true);
    }
    [ContextMenu("post event: hide")]
    private void testPostHidevent()
    {
        RedDotManager.getInstance().postEvent(type, false);
    }
#endif
}

