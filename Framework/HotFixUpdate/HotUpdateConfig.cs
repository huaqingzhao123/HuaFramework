using HuaFramework.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace HuaFramework.ResourcesRef
{
    public class HotUpdateConfig
    {
        public virtual string HotUpdateAssetBundleFolder
        {
            get { return Application.persistentDataPath + "/AssetBundles/"; }
        }
        /// <summary>
        /// 本地ResVersion文件
        /// </summary>
        public virtual string LocalResversionFilePath
        {
            get { return Application.streamingAssetsPath + "/AssetBundles/" + HotResUtil.GetPlatformName() + "/" + HotUpdateManager.ResVersionName; }
        }

        /// <summary>
        ///本地AB包根目录
        /// </summary>
        public virtual string LocalAssetBundleFolder
        {
            get { return Application.streamingAssetsPath + "/AssetBundles/" + HotResUtil.GetPlatformName() + "/"; }
        }
        /// <summary>
        /// 本地下载数据的临时目录,防止断网等意外造成文件损坏
        /// </summary>
        public virtual string TempAssetBundlePath
        {
            get { return Application.persistentDataPath + "Temp/"; }
        }
        public virtual string RemoteAssetBundlePathBase
        {
            get { return Application.dataPath + "/HuaFramework/Framework/HotFixUpdate/Remote/"; }
        }
        public virtual string RemoteResVersionPath
        {
            get { return Application.dataPath + "/HuaFramework/Framework/HotFixUpdate/Remote/" + HotUpdateManager.ResVersionName; }
        }
        /// <summary>
        /// 读取本地数据中的ResVersion文件
        /// </summary>
        /// <returns></returns>
        public virtual ResVersion LoadHotUpdateAssetBundleFolderResVersion()
        {
            var perisistentResVersionFilePath = HotUpdateAssetBundleFolder + HotUpdateManager.ResVersionName;
            //文件不存在则直接返回null
            if (!File.Exists(perisistentResVersionFilePath))
                return null;
            var persistResVersionJson = File.ReadAllText(perisistentResVersionFilePath);
            var persistResVersion = JsonUtility.FromJson<ResVersion>(persistResVersionJson);
            return persistResVersion;
        }

        public virtual IEnumerator GetStreamingAssetsResVersion(Action<ResVersion> onGetLocalResVersion)
        {
            var resVersionPath = LocalAssetBundleFolder + HotUpdateManager.ResVersionName;
            WWW www = new WWW(resVersionPath);
            yield return www;
            var resVersion = JsonUtility.FromJson<ResVersion>(www.text);
            onGetLocalResVersion?.Invoke(resVersion);
        }

        public virtual IEnumerator DoDownloadRes(ResVersion resVersion, Action downloadDone)
        {
            if (!Directory.Exists(TempAssetBundlePath))
            {
                Directory.CreateDirectory(TempAssetBundlePath);
            }
            var tempResVersionPath = TempAssetBundlePath + HotUpdateManager.ResVersionName;
            var resVersionJson = JsonUtility.ToJson(resVersion);
            File.WriteAllText(tempResVersionPath, resVersionJson);
            var remoteBasePath = RemoteAssetBundlePathBase;
            resVersion.AllAssetBundles.Add(HotResUtil.GetPlatformName());
            foreach (var assetBundle in resVersion.AllAssetBundles)
            {
                //下载各个AssetBundle
                var path = remoteBasePath + assetBundle;
                WWW www = new WWW(path);
                yield return www;
                var bytes = www.bytes;
                var tempPath = TempAssetBundlePath + assetBundle;
                File.WriteAllBytes(tempPath, bytes);
            }
            if (downloadDone != null)
                downloadDone.Invoke();
            CommonUtil.OpenSpecificDirectory(Application.persistentDataPath);
        }
        /// <summary>
        /// 模拟请求服务器资源版本
        /// </summary>
        /// <param name="onResDownloaded"></param>
        /// <returns></returns>
        public virtual IEnumerator RequestResRemoteVersion(Action<ResVersion> onResDownloaded)
        {
            var remoteResVersionPath = RemoteResVersionPath;
            WWW www = new WWW(remoteResVersionPath);
            yield return www;
            var jsonString = www.text;
            var resVersion = JsonUtility.FromJson<ResVersion>(jsonString);
            onResDownloaded?.Invoke(resVersion);
        }
    }

}
