#if !ASSET_RESOURCES && (!UNITY_IOS || USE_SUB_PACKAGE) 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nireus
{
    public interface IUpdateClient
    {
        void OnWebPatchInfoError();
        void OnWebPatchNeedDownload(Version version);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="updated">true已经更新成功，false为不需要更新</param>
        void OnAllUpdateSucc(string file_name,string version);
        void OnDontNeedUpdate();
        void OnDownloadSucc(Version version);
        void OnDownloadError(Version version, float max_size_m);
        void OnDownloadProgress(Version version, float progress,float m, float max_size_m);
        void OnDownloadStart(Version version);
    }
}
#endif