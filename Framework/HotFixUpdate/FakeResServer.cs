using HuaFramework.Singleton;
using HuaFramework.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace HuaFramework.ResourcesRef
{
    [Serializable]
    public class ResVersion
    {
        public int Version = 0;
        public List<string> AllAssetBundles;
    }


    /// <summary>
    /// 热更
    /// </summary>
    public class FakeResServer : MonoSingleton<FakeResServer>
    {
   
        public void GetRemoteResVersion(Action<int> onRemoteResVersionGet)
        {
            StartCoroutine(HotUpdateManager.Instance.HotUpdateConfig.RequestResRemoteVersion(resVersion => onRemoteResVersionGet?.Invoke(resVersion.Version)));
        }

        /// <summary>
        /// 异步从服务器下载资源
        /// </summary>
        /// <param name="onResDownloaded"></param>
        public void DownloadRes(Action onResDownloaded)
        {
            StartCoroutine(HotUpdateManager.Instance.HotUpdateConfig.RequestResRemoteVersion(ResVersion =>
            {
                StartCoroutine(HotUpdateManager.Instance.HotUpdateConfig.DoDownloadRes(ResVersion, onResDownloaded));
            }));
        }



      

    }


}
