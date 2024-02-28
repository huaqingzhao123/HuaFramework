#if !UNITY_IOS || USE_SUB_PACKAGE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nireus
{
#if UNITY_ANDROID || USE_SUB_PACKAGE
    class AssetBundleSystem : IDownloadQueue
#else
    class AssetBundleSystem
#endif
    {
        private int _total_file_cnt;
        private int _cur_file_cnt;
        IAssetBundleManager _update_client;
        private AssetResManifest _local_manifest;       // 当前版本;
        public AssetResManifest localManifest() { return _local_manifest; }
        public AssetResManifest remoteManifest() { return _remote_manifest; }
        private static AssetResManifest _remote_manifest;    // 最新版本;

        private string _storage_path = FilePathHelper.PATCH_GAME_RES_DIR;
        private string _temp_storage_path = FilePathHelper.PATCH_TEMP_ROOT_DIR;

        private Version _server_version;
        DownloadQueue download_queue;
        private int _try_cnt = 0;
        private Dictionary<string,DownloadUnit> _download_fail_dic = new Dictionary<string,DownloadUnit>();
        public AssetBundleSystem(IAssetBundleManager client)
        {
#if UNITY_ANDROID || USE_SUB_PACKAGE
            GameObject go = new GameObject("res_downloader");
            download_queue = go.AddComponent<DownloadQueue>();
            download_queue.SetMaxLoader(1);
            download_queue.SetOwner(this);
            _update_client = client;
            Init();
#endif
        }

#if UNITY_ANDROID || USE_SUB_PACKAGE
        private void Init()
        {
            string file_data = FileUtil.getInstance().LoadStreamOrPatchTextAsset(PathConst.ASSETS_VERSION_INFO_FILENAME);
            if (string.IsNullOrEmpty(file_data) == false)
            {
                _local_manifest = AssetResManifest.parseFile(file_data);
            }
        }
        
        public void StartUpdateVersionFile_Module(string version, string url,AssetResManifest local_manifest, string packageName)
        {
            _try_cnt = 0;
            _download_fail_dic.Clear();
            _local_manifest = local_manifest;
            _storage_path = Path.Combine(FilePathHelper.PATCH_GAME_RES_MODULE_DIR,packageName);
            if (string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(url))
            {
                _update_client.OnDontNeedUpdate();
                return;
            }
            _server_version = new Version(version);
            if (_local_manifest == null || _server_version.isNewerThan(_local_manifest.version))
            {
                var patch = Path.Combine(url, version, packageName);
                _server_version.setPatchUrl(patch);
                _remote_manifest = null;
                DownloadVersion();
            }
            else
            {
                _update_client.OnDontNeedUpdate();
                return;
            }

        }
        public void StartUpdateVersionFile(string version, string url)
        {
            _try_cnt = 0;
            _download_fail_dic.Clear();
            _storage_path = FilePathHelper.PATCH_GAME_RES_DIR;
            if (string.IsNullOrWhiteSpace(version) || string.IsNullOrWhiteSpace(url) || _local_manifest == null)
            {
                _update_client.OnDontNeedUpdate();
                return;
            }
            _server_version = new Version(version);
            if (_server_version.isNewerThan(_local_manifest.version))
            {
                var path = Path.Combine(url, version);
                _server_version.setPatchUrl(path);
                DownloadVersion();
            }
            else
            {
                _update_client.OnDontNeedUpdate();
                return;
            }

        }

        private void DownloadVersion()
        {
            _total_file_cnt = 1;
            _cur_file_cnt = 0;
            _update_client.OnVersionFileNeedDownload(_server_version);
        }

        public void StartDownloadVersion(MonoBehaviour mono)
        {
            try
            {
                mono.StartCoroutine(DownloadPatch_IE(mono, _server_version));
            }
            catch (Exception)
            {
                _update_client.OnDownloadVersionError(_server_version, 0);
            }
        }

        IEnumerator DownloadPatch_IE(MonoBehaviour mono, Nireus.Version version)
        {
            //LogService.LogPatchRes("1");
            string patch_file_url = Path.Combine(version.getPatchUrl(), PathConst.ASSETS_VERSION_INFO_FILENAME);
            _update_client.OnDownloadStart(version);

            FileDownloader downloader = new FileDownloader();
            downloader.OnProgress = (progress, cur_byte, max_byte) =>
            {
                float max_size_m = version.getLength() / (1024 * 1000f);
                float m = (progress * version.getLength()) / (1024 * 1000f);
                _update_client.OnDownloadProgress(progress, m, max_size_m);
            };
            yield return mono.StartCoroutine(downloader.DownloadByWWW(patch_file_url, false, false));
            if (string.IsNullOrEmpty(downloader.error_code) == false)//error
            {
                float max_size_m = version.getLength() / (1024 * 1000f);
                _update_client.OnDownloadVersionError(version, max_size_m);
                yield break;
            }
            else
            {
                //LogService.LogPatchRes("2");

                _remote_manifest = AssetResManifest.parseFile(downloader.FileData);
                if (_remote_manifest == null)
                {
                    _update_client.OnDownloadVersionError(version, 0);
                    yield break;
                }
                else
                {
                    if (_local_manifest!= null && _local_manifest.versionGreaterOrEquals(ref _remote_manifest))
                    {
                        _update_client.OnDontNeedUpdate();
                        yield break;
                    }
                    if (Directory.Exists(_temp_storage_path))
                    {
                        Directory.Delete(_temp_storage_path,true);
                    }
                    Directory.CreateDirectory(_temp_storage_path);
                    string file_path = Path.Combine(_temp_storage_path, PathConst.ASSETS_VERSION_INFO_FILENAME);
                    FileUtil.getInstance().saveFile(file_path, downloader.FileData);
                    _update_client.OnDownloadVersionSucc(version);
                }

            }
        }

        public void Replaceversionfile()
        {
            string file_path = Path.Combine(_temp_storage_path, PathConst.ASSETS_VERSION_INFO_FILENAME);
            if (File.Exists(file_path))
            {
                string dest_file_path = Path.Combine(_storage_path, PathConst.ASSETS_VERSION_INFO_FILENAME);
                if(File.Exists(dest_file_path))
                {
                    File.Delete(dest_file_path);
                }
                var file = new FileInfo(file_path);
                file.CopyTo(dest_file_path);
            }
        }

        public float GetDownLoadResSize()
        {
            if (_remote_manifest == null)
            {
                return 0;
            }
            Dictionary<string, DownloadUnit> update_map = new Dictionary<string, DownloadUnit>();
            Dictionary<string, DownloadUnit> delete_map = new Dictionary<string, DownloadUnit>();
            if (_local_manifest == null)
            {
                _remote_manifest.getNeedUpdateAssetBundle(ref update_map);
            }
            else
            {
                _local_manifest.getNeedUpdateOrDeleteAssetBundle(ref _remote_manifest, ref update_map, ref delete_map);
            }
            if (update_map.Count == 0 && delete_map.Count == 0)
            {
                return 0;
            }
            
            long max_bytes = 0;
            foreach (var pair in update_map)
            {
                string name = pair.Key;
                DownloadUnit unit = pair.Value;
                max_bytes += unit.size;
                //GameDebug.LogError("AssetBundleSystem NeedDownloadRes file = " + name + ",length = " + unit.size);
            }

            return max_bytes;
        }
        public void StartDownloadRes(MonoBehaviour mono)
        {
            _download_fail_dic.Clear();
            Dictionary<string, DownloadUnit> update_map = new Dictionary<string, DownloadUnit>();
            Dictionary<string, DownloadUnit> delete_map = new Dictionary<string, DownloadUnit>();
            if (_local_manifest == null)
            {
                _remote_manifest.getNeedUpdateAssetBundle(ref update_map);
            }
            else
            {
                _local_manifest.getNeedUpdateOrDeleteAssetBundle(ref _remote_manifest, ref update_map, ref delete_map);
            }
            if (update_map.Count == 0 && delete_map.Count == 0)
            {
                AllUpdateSucc();
                return;
            }
            foreach (var pair in delete_map)
            {
                string name = pair.Key;
                DownloadUnit unit = pair.Value;
                string file_path = Path.Combine(_storage_path, name);
                FileUtil.getInstance().deleteFile(file_path);
            }
            long max_bytes = 0;
            bool is_had_file_downloading = download_queue.IsHadFileDownloading();
            if (is_had_file_downloading == false)
            {
                download_queue.CancelQueue();
            }
            foreach (var pair in update_map)
            {
                string name = pair.Key;
                DownloadUnit unit = pair.Value;
                max_bytes += unit.size;
                GameDebug.LogError("AssetBundleSystem StartDownloadRes file = " + name + ",length = " + unit.size);
                string url = Path.Combine(_server_version.getPatchUrl(), unit.name);
                download_queue.AddQueue(url, unit.size, unit, 0);
            }
            download_queue.StartDownload();
        }
        public void resumeDownloadFailFile()
        {
            if(_try_cnt >= 3)
            {
                _update_client.OnDownloadError("fail");
                return;
            }
            _try_cnt++;
            bool is_had_file_downloading = download_queue.IsHadFileDownloading();
            if (is_had_file_downloading == false)
            {
                download_queue.CancelQueue();
            }
            foreach (var pair in _download_fail_dic)
            {
                string name = pair.Key;
                DownloadUnit unit = pair.Value;
                GameDebug.LogError("AssetBundleSystem resumeDownloadFailFile file = " + name + ",length = " + unit.size);
                string url = Path.Combine(_server_version.getPatchUrl(), unit.name);
                download_queue.AddQueue(url, unit.size, unit, 0);
            }
            download_queue.StartDownload();
        }
        public bool HadDownloadFailFile()
        {
            return _download_fail_dic.Count > 0;
        }
        public void OnDownloadAllSucc(object user_data)
        {
            download_queue.CancelQueue();
            if(_download_fail_dic.Count > 0)
            {
                resumeDownloadFailFile();
            }
            else
            {
                AllUpdateSucc();
            }
        }
        private void AllUpdateSucc()
        {
            if (Directory.Exists(_temp_storage_path))
            {
                if (!Directory.Exists(FilePathHelper.PATCH_AB_ROOT_DIR))
                {
                    Directory.CreateDirectory(FilePathHelper.PATCH_AB_ROOT_DIR);
                }
                if (!Directory.Exists(_storage_path))
                {
                    Directory.CreateDirectory(_storage_path);
                }
                DirectoryInfo dir = new DirectoryInfo(_temp_storage_path);
                FileInfo[] files = dir.GetFiles();
                for (int i = 0; i < files.Length; ++i)
                {
                    var file = files[i];
                    string dest_file_path = Path.Combine(_storage_path, file.Name);
                    if(File.Exists(dest_file_path))
                    {
                        File.Delete(dest_file_path);
                    }
                    file.CopyTo(dest_file_path);
                }
                Directory.Delete(_temp_storage_path,true);
            }
            _update_client.OnAllUpdateSucc();
        }
        public void OnDownloadGroupSucc(object user_data, int group_id)
        {
            //todo
        }

        public void OnDownLoadOneSucc(string url, byte[] bytes, object user_data, int group_id)
        {
            DownloadUnit unit = user_data as DownloadUnit;
            string full_name = Path.Combine(_temp_storage_path, unit.name);
            if(CheckFileValid(unit,bytes))
            {
                CreateFile(full_name, bytes);
                if(_download_fail_dic.ContainsKey(unit.name))
                {
                    _download_fail_dic.Remove(unit.name);
                }
            }
            else
            {
                if(!_download_fail_dic.ContainsKey(unit.name))
                {
                    _download_fail_dic.Add(unit.name, unit);
                } 
            }
        }
        private bool CheckFileValid(DownloadUnit unit, byte[] file_data)
        {
            
            var download_md5 = Encryption.md5(file_data);
            var md5 = unit.md5.ToUpper();
            if(download_md5.CompareTo(md5) != 0)
            {
                GameDebug.LogError("AssetBundleSystem CheckFileValid file = " + unit.name + ",length = " + unit.size +",md5=" + md5 + ",download_length =" + file_data.Length + ",download_md5 = " + download_md5);
            }
            return download_md5.CompareTo(md5) == 0;
        }
        private void CreateFile(string file_path, byte[] file_data)
        {
            if (File.Exists(file_path))
            {
                File.Delete(file_path);
            }
            using (FileStream streamWriter = File.Create(file_path))
            {
                streamWriter.Write(file_data, 0, file_data.Length);
            }
        }
        public void OnError(string s, object user_data)
        {
            download_queue.CancelQueue();
            download_queue.StopDownload();
            _update_client.OnDownloadError(s);
        }

        public void OnProgress(float p, int cur_byte, long max_byte, object user_data)
        {
            _update_client.OnDownloadProgress(p, (float)cur_byte / 1024000.0f, (float)max_byte / 1024000.0f);
        }
#endif
    }

}
#endif