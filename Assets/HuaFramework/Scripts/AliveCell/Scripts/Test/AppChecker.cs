/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/11/21 0:19:28
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// AppChecker
    /// </summary>
    public class AppChecker : MonoBehaviour
    {
        [SerializeField]
        protected List<GameObject> waittingForInited = null;

        protected void Awake()
        {
            if (!App.isInited && waittingForInited.Count > 0)
            {
                StartCoroutine(OnWaittingForInited());
            }
        }

        private IEnumerator OnWaittingForInited()
        {
            int count = waittingForInited.Count;
            bool[] status = new bool[count];

            for (int i = 0; i < count; i++)
            {
                GameObject obj = waittingForInited[i];
                status[i] = obj.activeSelf;
                if (obj.activeSelf)
                {
                    obj.SetActive(false);
                }
            }

            yield return new WaitUntil(() => App.isInited);

            for (int i = 0; i < count; i++)
            {
                GameObject obj = waittingForInited[i];
                if (status[i])
                {
                    obj.SetActive(true);
                }
            }
        }
    }
}