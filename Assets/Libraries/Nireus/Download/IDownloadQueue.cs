#if !ASSET_RESOURCES && (!UNITY_IOS || USE_SUB_PACKAGE) 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nireus
{

    public interface IDownloadQueue
    {
        void OnDownloadAllSucc(object user_data);
        void OnDownloadGroupSucc(object user_data,int group_id);

        void OnDownLoadOneSucc(string url, byte[] datas, object user_data,int group_id);

        void OnError(string s, object user_data);

        void OnProgress(float p, int cur_byte, long max_byte, object user_data);
    }

}
#endif