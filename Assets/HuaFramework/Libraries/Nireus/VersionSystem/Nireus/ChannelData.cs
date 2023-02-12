using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nireus
{
#if !ASSET_RESOURCES && (!UNITY_IOS || USE_SUB_PACKAGE) 
    public class ChannelData
    {
        public string channel_name;
        public string channel_code;
        public string channel_subcode;
        public string last_client_version;
        public string last_res_version;
        public string patch_url_root;
        public string android_apk_url;
        public string ios_ipa_url;
    }

    public class VersionResultInfo 
    {
        public string last_res_version          ;
        public string last_client_version       ;
        public string patch_url                 ;
        public string app_url                   ;
        public int    patch_size                ;
        public string patch_md5                 ;
        public bool   is_need_download_app		;
	    public bool   is_need_download_res		;
        public string login_server_address      ;
        public int    include_server_type       ;
    }
#endif
}