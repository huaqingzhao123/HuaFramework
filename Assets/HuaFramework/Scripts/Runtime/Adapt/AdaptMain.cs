using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 挂载此脚本时会自动读取身上的位置和scale作为默认值，故应该设置正确后再挂载
/// </summary>
public class AdaptMain : MonoBehaviour
{
    public Vector3 pos1280 = Vector3.zero;
    public Vector2 Scale1280;
    public Vector3 pos1024 = Vector3.zero;
    public Vector2 Scale1024;
    private Vector3 scale;
    public Vector3 originalPos = Vector3.zero;
    public Vector3 originalScale = Vector3.zero;

    void Awake()
    {
        //Set();
    }
    private void Reset()
    {
        originalPos = transform.localPosition;
        originalScale = transform.localScale;
    }
    /// <summary>
    /// 主动设置，防止Awake控制需要考虑和游戏内控制执行顺序问题
    /// </summary>
    /// <param name="width"></param>
    public void Set(int width = 0)
    {
        int screenWidth;
        if (width != 0)
        {
            screenWidth = width;
        }
        else
        {
            screenWidth = Screen.width;
        }
        scale = transform.localScale;
        //this.transform.localPosition = Vector3.zero;
        if (screenWidth == 1024)
        {
            if (Mathf.Abs(Scale1024.x) > 0.05f)
                transform.localScale = new Vector3(Scale1024.x, Scale1024.y, scale.z);
            if (pos1024 != Vector3.zero)
                transform.localPosition = pos1024;
        }
        else if (screenWidth == 1280)
        {
            if (Mathf.Abs(Scale1280.x) > 0.05f)
                transform.localScale = new Vector3(Scale1280.x, Scale1280.y, scale.z);
            if (pos1280 != Vector3.zero)
                transform.localPosition = pos1280;
        }
        else
        {
            if (originalScale != Vector3.zero)
            {
                transform.localScale = originalScale;
            }
            if (originalPos != Vector3.zero)
                transform.localPosition = originalPos;
        }
    }
}

