using HuaFramework.Managers;
using HuaFramework.ResourcesManager;
using HuaFramework.Singleton;
using HuaFramework.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public class FrameWorkExample : MonoSingleton<FrameWorkExample>
{
    ResourceLoader resourceLoader;
    private void Start()
    {
        //GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Delay(3, () => Debug.LogError("3s我来了"));
        Action action = () => Debug.LogError("HelloWorld!");
        RegisterMessage("test", action);
        //NewSendMessage("test");
        Action action2 = () => Debug.LogError("第二次Hello");
        RegisterMessage("test", action2);
        //NewSendMessage("test");
        //UnRegisterExpactMessage("test", action);
        //NewSendMessage("test");
        //UnRegisterSelectedAllMessage("test");
        NewSendMessage("test");

        //AudioManager.Instance.PlayCommonAudio("蹦跳");
        //AudioManager.Instance.PlayBgAudio("蹦跳");

        resourceLoader = new ResourceLoader();
        //resourceLoader.LoadAssetsAsync<AudioClip>(ResourceLoader.ResourcesAssetsPrefix + "Audio/蹦跳",(asset)=>Debug.LogError("加载成功"));
        //resourceLoader.LoadAssetsSync<AudioClip>(ResourceLoader.ResourcesAssetsPrefix + "Audio/蹦跳");
        //Debug.LogError(Application.streamingAssetsPath);
        //resourceLoader.LoadAssetsAsync<AssetBundle>(Application.streamingAssetsPath + "/assetbundleone", (assetBundle) =>
        //{
        //    Debug.LogError("第一次获得ab包");
        //    var gameObject2 = Instantiate(assetBundle.LoadAsset<GameObject>("asset1"));
        //});
        // resourceLoader.LoadAssetsAsync<AssetBundle>(Application.streamingAssetsPath + "/assetbundleone", (assetBundle) =>
        //{
        //    Debug.LogError("第二次获得ab包");
        //});
        //resourceLoader.LoadAssetsAsync<AssetBundle>(Application.streamingAssetsPath + "/audio", (assetBundle) =>
        //{
        //    Debug.LogError("第一次获得audio");
        //});

        //resourceLoader.LoadAssetsAsync<GameObject>(Application.streamingAssetsPath + "/assetbundleone", "asset1", (asset) =>
        //{
        //    Debug.LogError("第一次获得ab包");
        //    var gameObject2 = Instantiate(asset);
        //    gameObject2.name = "AsyncGameobject";
        //});
  
        Debug.Log("当前平台的AssetBundle打包路径:" + HotUpdateManager.Instance.HotUpdateConfig.LocalAssetBundleFolder);

        //var path = "Assets/HuaFramework/Framework/HotFixUpdate/ResVersion.json";
        //ResVersion resVersion = new ResVersion() { Version = 1 };
        //var jsonString = JsonUtility.ToJson(resVersion);
        //File.WriteAllText(path, jsonString);
        //AssetDatabase.Refresh();

        HotUpdateManager.Instance.CheckState(()=> {

            HotUpdateManager.Instance.HasNewVersionRes(needUpdate=> {

                if (needUpdate)
                {
                    HotUpdateManager.Instance.UpdateRes(()=>{
                    AssetBundleManifestData.Instace.Load();
                    var obj1 = Instantiate(resourceLoader.LoadAssetsSync<GameObject>("assetbundleone", "asset1"));
                    obj1.name = "SyncGameobject";
                    resourceLoader.LoadAssetsAsync<GameObject>("assetbundleone", "asset1", asset => {
                        var obj2 = Instantiate(asset);
                        obj2.name = "AsyncGameobject";
                    });
                    resourceLoader.LoadAssetsSync<AssetBundle>("audio");
                });
                    Debug.LogError("需要热更新");
                }
                else
                {
                    Debug.LogError("不需要热更新");
                }
  
            });
        });
    }
    private void OnDestroy()
    {
        resourceLoader.ReleaseAllAssets();
    }
}
