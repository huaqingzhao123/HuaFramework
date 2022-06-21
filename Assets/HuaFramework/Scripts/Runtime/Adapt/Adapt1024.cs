using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Adapt1024 : MonoBehaviour
{
    private void Reset()
    {
        var childAdapt = GetComponentsInChildren<AdaptMain>(true);
        for (int i = 0; i < childAdapt.Length; i++)
        {
            childAdapt[i].Set(1024);
            //Debug.LogError("设置了:" + childAdapt[i].name);
        }
    }
    private void Awake()
    {
        if (Screen.width != 1024) return;
        //直接设置子物体的适配，不影响游戏中对其更改
        var childAdapt = GetComponentsInChildren<AdaptMain>(true);
        for (int i = 0; i < childAdapt.Length; i++)
        {
            childAdapt[i].Set(1024);
            //Debug.LogError("设置了:" + childAdapt[i].name);
        }
    }
}
