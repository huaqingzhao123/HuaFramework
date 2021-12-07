using HuaFramework.Singleton;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HuaFramework.ResourcesManager
{
    /// <summary>
    /// 资源状态
    /// </summary>
    public enum ResState
    {

        /// <summary>
        /// 从未更新过
        /// </summary>
        NerverUpdate,
        /// <summary>
        /// 更新过
        /// </summary>
        Updated,
        /// <summary>
        /// 覆盖apk更新
        /// </summary>
        Overrided,
    }
    /// <summary>
    /// 热更管理类
    /// </summary>
    public class HotUpdateManager : MonoSingleton<HotUpdateManager>
    {
        public const string ResVersionName = "ResVersion.json";

        private ResState _resState;
        public ResState ResState
        {
            get { return _resState; }
        }

        public HotUpdateConfig HotUpdateConfig
        {
            get;set;
        }
        private void Awake()
        {
            HotUpdateConfig = new HotUpdateConfig();
        }
        public void CheckState(Action done)
        {
            var peisistentResVersion =HotUpdateConfig.LoadHotUpdateAssetBundleFolderResVersion();
            if (peisistentResVersion==null)
            {
                _resState = ResState.NerverUpdate;
                done?.Invoke();
            }
            else
            {
                StartCoroutine(HotUpdateConfig.GetStreamingAssetsResVersion(localVersion =>
                {
                    //比较本地资源中的版本和项目中的资源版本
                    if (peisistentResVersion.Version > localVersion.Version)
                    {
                        _resState = ResState.Updated;
                    }
                    else
                    {
                        _resState = ResState.Overrided;
                    }
                    done?.Invoke();
                }));

            }

        }



        private void GetLocalResVersion(Action<int> onGetLocalResVersion)
        {
            if (_resState == ResState.NerverUpdate || _resState == ResState.Overrided)
            {
                //此时本地的resVersion为StreamingAssets下的
                StartCoroutine(HotUpdateConfig.GetStreamingAssetsResVersion(streamingResVersion =>
                {
                    onGetLocalResVersion?.Invoke(streamingResVersion.Version);
                }));
                return;
            }
            //从Persistent路径下读
            var resVersion = HotUpdateConfig.LoadHotUpdateAssetBundleFolderResVersion();
            onGetLocalResVersion?.Invoke(resVersion.Version);
        }

        /// <summary>
        /// 检测服务器是否有新版本
        /// </summary>
        /// <param name="handleUpdate"></param>
        public void HasNewVersionRes(Action<bool> handleUpdate)
        {
            FakeResServer.Instance.GetRemoteResVersion(remoteVersion =>
            {
                GetLocalResVersion(localVersion =>
                {
                    handleUpdate?.Invoke(remoteVersion > localVersion);
                });
            });
        }

        /// <summary>
        /// 需要从服务器更新资源,异步操作
        /// </summary>
        public void UpdateRes(Action onComplete)
        {
            Debug.Log("开始更新资源");
            Debug.Log("下载资源");
            FakeResServer.Instance.DownloadRes(() =>
            {
                RepalceLocalRes();
                _resState = ResState.Updated;
                Debug.Log("结束更新");
                onComplete?.Invoke();
            });
        }
        /// <summary>
        /// 替换本地资源
        /// </summary>
        void RepalceLocalRes()
        {
            Debug.Log("替换本地资源");
            var tempAssetBundleFolder = HotUpdateConfig.TempAssetBundlePath;
            var persistentAssetBundleFolder = HotUpdateConfig.HotUpdateAssetBundleFolder;
            if (Directory.Exists(persistentAssetBundleFolder))
                Directory.Delete(persistentAssetBundleFolder, true);
            Directory.Move(tempAssetBundleFolder, persistentAssetBundleFolder);
            if (Directory.Exists(tempAssetBundleFolder))
                Directory.Delete(tempAssetBundleFolder, true);
        }
    }


}
