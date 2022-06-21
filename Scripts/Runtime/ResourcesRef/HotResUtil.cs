using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.ResourcesRef
{

    /// <summary>
    /// 热更资源工具类
    /// </summary>
    public class HotResUtil
    {

   

        /// <summary>
        /// persistent下AB包根目录
        /// </summary>
        //public static string PersistentAssetBundleRootPath
        //{
        //    get { return Application.persistentDataPath + "/AssetBundles/"; }
        //}
        public static string GetOneAssetBundlePath(string assetBundleName)
        {
            if (HotUpdateManager.Instance.ResState == ResState.NerverUpdate
                || HotUpdateManager.Instance.ResState == ResState.Overrided)
                return HotUpdateManager.Instance.HotUpdateConfig.LocalAssetBundleFolder+ assetBundleName;
            return HotUpdateManager.Instance.HotUpdateConfig.HotUpdateAssetBundleFolder + assetBundleName;
        }
        /// <summary>
        /// 获得当前平台的名字
        /// </summary>
        public static string GetPlatformName()
        {

#if UNITY_EDITOR
            return EditorGetPlatformName(UnityEditor.EditorUserBuildSettings.activeBuildTarget);
#else
            return RuntimeGetPlatformName(Application.platform);

#endif
        }
#if UNITY_EDITOR
        private static string EditorGetPlatformName(UnityEditor.BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case UnityEditor.BuildTarget.StandaloneOSX:
                    return "OSX";
                case UnityEditor.BuildTarget.StandaloneWindows:
                    return "StandaloneWindows";
                case UnityEditor.BuildTarget.iOS:
                    return "iOS";
                case UnityEditor.BuildTarget.Android:
                    return "Android";
                case UnityEditor.BuildTarget.StandaloneLinux:
                    return "Linux";
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    return "Windows64";
                case UnityEditor.BuildTarget.WebGL:
                    return "WebGL";
                case UnityEditor.BuildTarget.WSAPlayer:
                    return "WSAPlayer";
                case UnityEditor.BuildTarget.StandaloneLinux64:
                    return "Linux64";
                case UnityEditor.BuildTarget.StandaloneLinuxUniversal:
                    return "LinuxUniversal";
                case UnityEditor.BuildTarget.XboxOne:
                    return "XboxOne";
                case UnityEditor.BuildTarget.tvOS:
                    return "tvOS";
                case UnityEditor.BuildTarget.NoTarget:
                    return "Windows";
                default:
                    return "Windows";
            }
        }

        /// <summary>
        ///本地AB包根目录
        /// </summary>
        public static string LocalAssetBundleFolder
        {
            get { return Application.streamingAssetsPath + "/AssetBundles/" + GetPlatformName() + "/"; }
        }
        public static string LocalResversionFilePath
        {
            get { return Application.streamingAssetsPath + "/AssetBundles/" + GetPlatformName() + "/" + HotUpdateManager.ResVersionName; }
        }
#endif
        private static string RuntimeGetPlatformName(RuntimePlatform runtimePlatform)
        {
            switch (runtimePlatform)
            {
                case RuntimePlatform.OSXPlayer:
                    return "OSX";
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                case RuntimePlatform.WindowsEditor:
                    return "Windows";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.LinuxPlayer:
                    return "Linux";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                case RuntimePlatform.XboxOne:
                    return "XboxOne";
                case RuntimePlatform.tvOS:
                    return "tvOS";
                default:
                    return "Windows";
            }
        }

    }


}
