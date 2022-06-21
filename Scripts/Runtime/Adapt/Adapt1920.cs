using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Adapt1920 : MonoBehaviour
{
    // 点击组件的设置按钮，点击Reset触发，注意reset对应分辨率时应将窗口调为对应分辨率
    //组件挂载的时候会先执行一次，注意设置值之前应该先挂载此脚本
    private void Reset()
    {
        var childAdapt = GetComponentsInChildren<AdaptMain>(true);
        for (int i = 0; i < childAdapt.Length; i++)
        {
            childAdapt[i].Set(1920);
            //Debug.LogError("设置了:" + childAdapt[i].name);
        }
    }
    private void Awake()
    {
        if (Screen.width != 1920) return;
        //直接设置子物体的适配，不影响游戏中对其更改
        var childAdapt = GetComponentsInChildren<AdaptMain>(true);
        for (int i = 0; i < childAdapt.Length; i++)
        {
            childAdapt[i].Set(1920);
            //Debug.LogError("设置了:" + childAdapt[i].name);
        }
    }
}
