using Nireus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nireus
{
    public class AssetLoaderManagerBase : IAssetLoaderReceiver
    {
        public Action OnLoadFinishedDelegate;
        public Action<float> OnLoadOneDelegate;
        protected List<AssetLoadInfo> _load_info_list; //启动时加载
        protected List<AssetLoadInfo> _load_info_deferred_list; //用到时加载
        protected List<string> _loaded_asset_path_list;
        //protected int _loaded_res_count = 0;
        private bool _is_loading = false;
        private bool _is_loaded = false;//TODO 应该用判断所有文件在资源表里的情况
        public bool is_loaded { get { return _is_loaded; } }
        public AssetLoaderManagerBase()
        {
            _load_info_list = new List<AssetLoadInfo>();
            _load_info_deferred_list = new List<AssetLoadInfo>();
            _loaded_asset_path_list = new List<string>();
        }

        public virtual void OnOneAssetLoaded(string assetPath, int curCount, int maxCount)
        {
            OnLoadOneDelegate?.Invoke(curCount / (float)maxCount);
            //LogUtils.LogUserTrace(LogUtils.GetLodingModuleTraceID(assetPath));
        }

        public virtual void OnLoadFinished()
        {
            OnLoadFinishedDelegate?.Invoke();
            OnLoadFinishedDelegate = null;
            OnLoadOneDelegate = null;
            //LogUtils.LogUserTrace(NewUserTrace.Loading_Complete);
        }

        public virtual void LoadModules(Action onLoadFinish, Action<float> onLoadOne)
        {
            OnLoadFinishedDelegate = onLoadFinish;
            OnLoadOneDelegate = onLoadOne;

            if (_is_loaded || _loaded_asset_path_list.Count == _load_info_list.Count)
            {
                OnLoadFinished();
                return;
            }

            if (_is_loaded || _is_loading) return;
            _is_loading = true;

            AssetManager.getInstance().loadAsync(_load_info_list);
            //AssetsLoadManager.Instance.LoadAsync(_load_info_list);
        }

        public void ReleaseAsset(string assetPath)
        {
            for (int i = 0; i < _loaded_asset_path_list.Count; i++)
            {
                var asset_path = _loaded_asset_path_list[i];
                if (asset_path == assetPath)
                {
                    _loaded_asset_path_list.RemoveAt(i);
                    _is_loaded = false;
                    break;
                }
            }
            AssetManager.getInstance().UnloadAsset(assetPath);
        }


        public void OnAssetLoadReceive(string assetPath, UnityEngine.Object asset_data, object info)
        {
            _loaded_asset_path_list.Add(assetPath);
            int loaded_res_count = _loaded_asset_path_list.Count;

            OnOneAssetLoaded(assetPath, loaded_res_count, _load_info_list.Count);

            if (loaded_res_count >= _load_info_list.Count)
            {
                postProcessWhenAllLoaded();

                OnLoadFinished();
				GameDebug.Log($"LoadingState on receive assetPath = {assetPath}, loaded_res_count = {loaded_res_count}, _load_info_list = {_load_info_list.Count}");
				//OnLoadFinishedDelegate?.Invoke();
    //            OnLoadFinishedDelegate = null;
    //            OnLoadOneDelegate = null;
            }
        }


        public void OnAssetLoadError(string url, object info)
        {
            GameDebug.LogErrorFormat("OnAssetLoadError:{0}", url);
        }

        public void OnAssetLoadProgress(string url, object info, float progress, int bytesLoaded, int bytesTotal)
        {

        }

        private void postProcessWhenAllLoaded()
        {
            initUIBatch(_load_info_list);
            _is_loading = false;
            _is_loaded = true;
            //  System.GC.Collect();
        }


        protected void addCommonRes(string path)
        {
            _load_info_list.Add(new AssetLoadInfo(path, null, this, AssetLoadType.COMMON));
        }

        protected void addPrefab(string path)
        {
            _load_info_list.Add(new AssetLoadInfo(PathUtils.GetPrefabPath(path), null, this, AssetLoadType.COMMON));
        }

        protected void addUIPrefab(string ui_path, Type ui_class = null, bool deferred = false)
        {
            var loadInfo = new AssetLoadInfo(PathUtils.GetUIPrefabPath(ui_path), ui_class, this,
                ui_class != null ? AssetLoadType.UI : AssetLoadType.COMMON);

            if (deferred)
            {
                _load_info_deferred_list.Add(loadInfo);
            }
            else
            {
                _load_info_list.Add(loadInfo);
            }
        }


        protected void addSceneRes(string sceneName)
        {
            _load_info_list.Add(new AssetLoadInfo(sceneName, null, this, AssetLoadType.SCENE));
        }

        private void initUIBatch(List<AssetLoadInfo> assetsList)
        {
            try
            {
                var list = new List<UITemplate>();
                foreach (var info in assetsList)
                {
                    if (info.type == AssetLoadType.UI)
                    {
                        // GameObject prefab = AssetManager.Instance.GetAsset<GameObject>(info.assetPath);
                        //
                        // var go = GameObject.Instantiate(prefab) as GameObject;
                        var go = PrefabLoadManager.Instance.LoadSync(info.assetPath);
                        go.name = info.GetPrefabName();
                        var uiClassType = info.info as Type;
                        if (uiClassType != null && go.GetComponent(uiClassType) == null)
                        {
                            var co = go.AddComponentIfNeeded(uiClassType);
                            list.Add(co as UITemplate);
                        }

                        //(newGo.transform as RectTransform).anchorMax = new Vector2(0.5f, 0.5f);
                        //(newGo.transform as RectTransform).anchorMin = new Vector2(0.5f, 0.5f);
                        go.transform.SetParent(LayerManager.getInstance().getLayer(LayerType.HIDE).transform, false);
                        go.SetActive(false);
                        //AssetManager.Instance.UnloadAssetDeferred(info.assetPath, true);
                    }
                }
                foreach (var co in list)
                {
                    if (co != null)
                    {
                        co.UIAwakeAllAfter(true, false);
                    }
                }
            }
            catch (Exception e)
            {
                GameDebug.LogError(e.ToString());
            }
        }

        public UITemplate LoadUI(Type uiClass)
        {
            try
            {
                var loadInfo = _load_info_deferred_list.Find((info => (Type)info.info == uiClass)) ??
                               _load_info_list.Find((info => (Type)info.info == uiClass));
                if (loadInfo == null) return null;
                
                var go = PrefabLoadManager.Instance.LoadSync(loadInfo.assetPath);
                
                if (go == null) return null;
                go.name = uiClass.Name;
                var comp = go.AddComponentIfNeeded(uiClass);
                comp.transform.SetParent(LayerManager.getInstance().getLayer(LayerType.HIDE).transform, false);
                go.SetActive(false);
                ((UITemplate) comp).UIAwakeAllAfter(true, false);
                return (UITemplate) comp;

            }
            catch (Exception e)
            {
                GameDebug.LogError(e.ToString());
                return null;
            }
        }
    }
}
