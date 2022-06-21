using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 控制子物体的适配
/// </summary>
public class AdaptController : MonoBehaviour
{

    private void Awake()
    {
        var childAdapt = GetComponentsInChildren<AdaptMain>(true);
        for (int i = 0; i < childAdapt.Length; i++)
        {
            childAdapt[i].Set(Screen.width);
            //Debug.LogError("设置了:" + childAdapt[i].name);
        }
    }
}
