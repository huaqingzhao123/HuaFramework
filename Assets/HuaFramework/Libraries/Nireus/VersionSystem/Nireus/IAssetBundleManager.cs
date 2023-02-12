using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nireus
{
#if !ASSET_RESOURCES && (!UNITY_IOS || USE_SUB_PACKAGE) 
    public interface IAssetBundleManager
    {
        void OnVersionFileNeedDownload(Version version);
        void OnAllUpdateSucc();
        void OnDontNeedUpdate();
        void OnDownloadVersionSucc(Version version);
        void OnDownloadError(string msg);
        void OnDownloadVersionError(Version version, float max_size_m);
        void OnDownloadProgress(float progress, float m, float max_size_m);
        void OnDownloadStart(Version version);
    }
#endif
}