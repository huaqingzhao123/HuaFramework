#if !ASSET_RESOURCES && (!UNITY_IOS || USE_SUB_PACKAGE) 
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Collections;

namespace Nireus
{
    public class VersionSystem
    {
        private int _total_file_cnt;
        private int _cur_file_cnt;

        IUpdateClient _update_client;
        private Version _local_last_res_version;       // 当前版本;
        public Version localLastResVersion() { return _local_last_res_version; }
        public Version serverLastResVersion() { return _server_last_res_version; }
        private static Version _server_last_res_version;    // 最新版本;


        public VersionSystem(IUpdateClient client,string version)
        {
            _update_client = client;
            _local_last_res_version = new Version(version);
        }

        public static Version GetServerLastResVersion()
        {
            return _server_last_res_version;
        }
        public void StartUpdataClient(string version,int length,string md5,string url,string file_path)
        {
            if(string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(url) || length <= 0 || string.IsNullOrWhiteSpace(md5))
            {
                _update_client.OnDontNeedUpdate();
                return;
            }
            _server_last_res_version = new Version(version);
            if(_server_last_res_version.isNewerThan(_local_last_res_version))
            {
                _server_last_res_version.setLength(length);
                _server_last_res_version.setMD5(md5);
                _server_last_res_version.setPatchUrl(url);
                _server_last_res_version.patch_path = file_path;
                DownloadPatch();
            }
            else
            {
                _update_client.OnDontNeedUpdate();
                return;
            }
            
        }
        
        void DownloadPatch()
        {
            _total_file_cnt = 1;
            _cur_file_cnt = 0;
            _update_client.OnWebPatchNeedDownload(_server_last_res_version);
        }


        public void StartDownload(MonoBehaviour mono)
        {
            try
            {
                mono.StartCoroutine(DownloadPatch_IE(mono,_server_last_res_version));
            }
            catch (Exception)
            {
                _update_client.OnDownloadError(_server_last_res_version,0);
            }
        }

        IEnumerator DownloadPatch_IE(MonoBehaviour mono,Nireus.Version version)
        {
            //LogService.LogPatchRes("1");
            string patch_file_url = serverLastResVersion().getPatchUrl();
            _update_client.OnDownloadStart(version);
            
            FileDownloader downloader = new FileDownloader();
            downloader.OnProgress = (progress,cur_byte,max_byte) =>               
            {
                float max_size_m = version.getLength() / (1024 * 1000f);
                float m = (progress * version.getLength()) / (1024 * 1000f);
                _update_client.OnDownloadProgress(version,progress,m,max_size_m);
            };
            yield return mono.StartCoroutine(downloader.DownloadByWWW(patch_file_url,false,false));
            if (string.IsNullOrEmpty(downloader.error_code) == false)//error
            {
                float max_size_m = version.getLength() / (1024 * 1000f);
                _update_client.OnDownloadError(version,max_size_m);
                yield break;
            }
            else
            {                              
                //LogService.LogPatchRes("2");
                _update_client.OnDownloadSucc(version);
                FileUtil.getInstance().saveFile(version.patch_path, downloader.FileData);
                _update_client.OnAllUpdateSucc(version.patch_path, version.ToString());

            }
        }



    }
}

#endif