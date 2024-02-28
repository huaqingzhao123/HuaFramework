using Nireus;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nireus
{
    //PrefabLoadManager相当于AssetsLoadManager的细化，只负责管理Assets中的Prefab
    //Prefab能被实例化和复制，PrefabLoadManager能管理引用，当所有实例都被销毁时，自动释放Prefab
    //-----------------------------------------------------------------------------------------------
    //每次调用Load加载Prefab实例时，会加载Asset并实例化（由于AssetsLoadManager的优化，不是每次都需要真正加载Asset）
    //实例化Prefab时，通过[PrefabAutoDestory]进行自动引用计数，当创建的实例被销毁时，引用计数减少
    //————————————————————————————————————————————
    //适用场景：使用频率低的资源（很少会使用，使用完也可以被释放）：比如一些很冷门的UI（比如荣耀竞技场竞猜面板）
    //使用频率很高的资源，建议通过AssetLoadManager直接获取预制，然后设置给GameObjectPool管理
    //————————————————————————————————————————————
    //GameObjectPool扩展：释放机制，几分钟不用，会被释放
    public class PrefabLoadManager
    {
        public static PrefabLoadManager Instance { get; } = new PrefabLoadManager();

        public delegate void PrefabLoadCallback(string name, GameObject obj);

        public bool log = false;

        class PrefabAutoDestory : MonoBehaviour
        {
            public static System.Action<string, int> on_awake;
            public static System.Action<string, int> on_destroy;
            public string asset_name;

            void Awake()
            {
                on_awake?.Invoke(asset_name, this.gameObject.GetInstanceID());
            }

            void OnDestroy()
            {
                on_destroy?.Invoke(asset_name, this.gameObject.GetInstanceID());
            }
        }

        class PrefabObject
        {
            public GameObject asset = null;
            public string asset_name = string.Empty;
            public Dictionary<PrefabLoadCallback, Transform> callback_list = new Dictionary<PrefabLoadCallback, Transform>();
            public int ref_count = 0;
            public bool loaded = false;
        }

        private Dictionary<string, PrefabObject> _loaded_list = new Dictionary<string, PrefabObject>();
        private Dictionary<string, PrefabObject> _loading_list = new Dictionary<string, PrefabObject>();
        private Dictionary<int, PrefabObject> _go_instance_id_list = new Dictionary<int, PrefabObject>(); 
        private GameObject _default_transfrom_parent;

        public PrefabLoadManager()
        {
            //本类没有Update函数，因为本类不像 AssetsLoadManager 或 AssetBundleLoadManager 那样需要检测 request.isDone 

            PrefabAutoDestory.on_awake = OnPrefabClone;
            PrefabAutoDestory.on_destroy = OnPrefabDestory;
            _default_transfrom_parent = new GameObject(nameof(PrefabLoadManager));
            UnityEngine.Object.DontDestroyOnLoad(_default_transfrom_parent);
        }

        public GameObject LoadSync(string asset_name, Transform tf_parent = null)
        {
            PrefabObject prefab_obj;
            if (_loaded_list.TryGetValue(asset_name, out prefab_obj))
            {
                _AddRef(prefab_obj);
                return _InstanceAsset(prefab_obj, tf_parent);
            }

            if(_loading_list.TryGetValue(asset_name, out prefab_obj))
            {
                //_loading_list.Remove(asset_name);不用去remove，因为异步加载成功回调会去移
                _AddRef(prefab_obj);
                //说明在[异步加载 LoadAsync]中，这里要再次调用 [同步加载 LoadSync] 转为同步加载
                //为了不让 Asset 的 [引用计数] 增多，等LoadSync完毕后，要卸载一次（不能反，反了asset为null)
                prefab_obj.asset = AssetManager.Instance.loadSync(asset_name) as GameObject;
                AssetManager.Instance.UnloadAsset(prefab_obj.asset.name,true,true);
            }
            else
            {
                prefab_obj = new PrefabObject();
                prefab_obj.asset_name = asset_name;
                prefab_obj.ref_count = 1;
                prefab_obj.asset = AssetManager.Instance.loadSync(asset_name) as GameObject;
                _OnPrefabLoaded(prefab_obj);
            }
            return _InstanceAsset(prefab_obj, tf_parent);
        }

        private void _OnPrefabLoaded(PrefabObject prefab_obj)
        {
            if (prefab_obj.loaded)
            {
                GameDebug.LogError($"[Prefab] [LoadManager] Asset has been loaded! {prefab_obj.asset_name}");
            }
            else
            {
                prefab_obj.loaded = true;
                _loaded_list.Add(prefab_obj.asset_name, prefab_obj);
                if (log) GameDebug.Log($"[Prefab] [LoadManager] Loaded: [{prefab_obj.asset_name}]");
                if (prefab_obj.asset == null)
                {
                    GameDebug.LogError($"[Prefab] [LoadManager] Asset is null: {prefab_obj.asset_name}");
                }
            }


            //触发回调
            foreach (var pair in prefab_obj.callback_list)
            {
                var go = _InstanceAsset(prefab_obj, pair.Value);
                try
                {
                    pair.Key?.Invoke(prefab_obj.asset_name, go);
                }
                catch (System.Exception e)
                {
                    GameDebug.LogError(e);
                }
                ////如果回调之后，节点挂在默认节点下，认为该节点无效，销毁
                //if (go.transform.parent == _default_transfrom_parent.transform)
                //{
                //    Object.Destroy(go);
                //}
            }
            prefab_obj.callback_list.Clear();
        }       

        private void _AddRef(PrefabObject prefab_obj)
        {
            prefab_obj.ref_count++;
            if (log) GameDebug.Log($"[Prefab] [LoadManager] Ref Count Increase To [{prefab_obj.ref_count}]: [{prefab_obj.asset_name}]");
        }

        private GameObject _InstanceAsset(PrefabObject prefab_obj, Transform parent)
        {
            if(prefab_obj.asset == null)
            {
                return null;
            }


            GameObject go = UnityEngine.Object.Instantiate(prefab_obj.asset, parent);
            _go_instance_id_list.Add(go.GetInstanceID(), prefab_obj);
       
            var prefab_auto_destroy = go.AddComponent<PrefabAutoDestory>();

            //active中的GameObject才能触发Awake
            //因为未Awake的脚本不能触发OnDestroy
            //我们需要通过Unity自动OnDestroy的机制来实现自动回收Prefab
            //只有Awake过的节点，才会被调用OnDestroy
            //所以为了确保Conponent被Awake，要挂在active的节点上！
            if (parent == null || parent.gameObject.activeInHierarchy == false)
            {
                go.transform.SetParent(_default_transfrom_parent.transform);
            }
            go.SetActive(true);

            //由于GameObject可能在外部被拷贝导致引用计数混乱，所以设计了【OnPrefabClone】
            //【OnPrefabClone】在【PrefabAutoDestory】【Awake】时会调用
            //但是现在是正常构造实例，而非外部克隆实例，所以本次不应该调用【OnPrefabClone】
            //为了达到这个目的，在【OnPrefabClone】内部加入判断【asset_name == null】的【if语句】
            //然后本处给asset_name赋值的代码，放了最后，此时【PrefabAutoDestory】已经【Awake】
            //这意味着【OnPrefabClone】已经执行，但是被return
            prefab_auto_destroy.asset_name = prefab_obj.asset_name;
            go.transform.SetParent(parent);
            return go;
        }

        private void OnPrefabClone(string asset_name, int instance_id)
        {
            //由于GameObject可能在外部被拷贝导致引用计数混乱
            //所以设置on_awake函数，在拷贝发生时更新引用计数
            if (asset_name == null)
            {
                return;
            }
            PrefabObject prefab_obj;
            _loaded_list.TryGetValue(asset_name, out prefab_obj);
            if(prefab_obj == null)
            {
                GameDebug.LogError($"[Prefab] [LoadManager] OnPrefabClone Error, AssetName: {asset_name}");
                return;
            }
            _AddRef(prefab_obj);

            if(_go_instance_id_list.ContainsKey(instance_id))
            {
                GameDebug.LogError($"[Prefab] [LoadManager] Already Contains InstanceId, AssetName: {asset_name}");
                return;
            }
            _go_instance_id_list.Add(instance_id, prefab_obj);
        }

        private void OnPrefabDestory(string asset_name, int instance_id)
        {
            PrefabObject prefab_obj;
            _loaded_list.TryGetValue(asset_name, out prefab_obj);
            if (prefab_obj == null)
            {
                GameDebug.LogError($"[Prefab] [LoadManager] OnPrefabClone Error, AssetName: {asset_name}");
                return;
            }
            prefab_obj.ref_count--;
            if (log) GameDebug.Log($"[Prefab] [LoadManager] Ref Count Decrease To [{prefab_obj.ref_count}]: [{prefab_obj.asset_name}]");
            _go_instance_id_list.Remove(instance_id);
            if (prefab_obj.ref_count <= 0)
            {
                if (prefab_obj.ref_count < 0)
                {
                    GameDebug.LogError($"[Prefab] [LoadManager] Destroy RefCount Error, AssetName: {prefab_obj.asset_name}");
                }
                if (log) GameDebug.Log($"[Prefab] [LoadManager] Unload: [{prefab_obj.asset_name}]");
                _loaded_list.Remove(prefab_obj.asset_name);
                if (AssetManager.Instance!=null)
                {
                    AssetManager.Instance.UnloadAsset(prefab_obj.asset_name);    
                }
                prefab_obj.asset = null;
            }
        }

       
    }
}