#if !ASSET_RESOURCES && (!UNITY_IOS || USE_SUB_PACKAGE) 
using System;
using Newtonsoft.Json.Linq;
using Nireus;
using UnityEngine;

public class RemoteVersionManager:Singleton<RemoteVersionManager>
{
    private Action _onGetVersionSucc;
    private bool _have_get_version = false;
    private string _version = "";
    private string _patch_url = "";

    public void InitGetRemoteVersion(Action getVersionSucc)
    {
        _onGetVersionSucc = getVersionSucc;
        if (_have_get_version)
        {
            this._onGetVersionSucc?.Invoke();
        }
        else
        {
            this.InitGetRemoteVersionImpl();
        }
    }
    
    private void InitGetRemoteVersionImpl()
    {
        //var param = new { version = VersionConfig.Instance.version, os = Platform.getDeviceType(), channel_id = PlatformHelper.GetU8Channel() };
#if !UNITY_IOS || USE_SUB_PACKAGE
        //GameService.CheckVersion(param, _OnCheckVersionSucc, _OnCheckVersionError);
#endif
    }
    
    private void _OnCheckVersionError(JObject data)
    {
        GameDebug.LogError("检查更新错误,远端版本获取失败，请重启游戏");
    }

    private void _OnCheckVersionSucc(JObject data)
    {
        if (data.TryGetValue("version", out var version) && data.TryGetValue("patch_url", out var patch_url))
        {
            GameDebug.Log("RemoteVersionManager manager _OnCheckVersionSucc " + version + " url " + patch_url);

            _have_get_version = true;
            _version = (string)version;
            _patch_url = (string) patch_url;
            
            this._onGetVersionSucc?.Invoke();
        }
        else
        {
            _OnCheckVersionError(data);
        }
    }

    public string GetPatchUrl()
    {
        return _patch_url;
    }
    
    public string GetRemoteVersion()
    {
        return _version;
    }
}
#endif