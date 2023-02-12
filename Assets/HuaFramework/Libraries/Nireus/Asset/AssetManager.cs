using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

/*
    目前对于界面的处理是  关闭后就删除  （TODO后期可加常驻界面）
    对于界面有一些规范
    
    1.对于这种写法
    private static DailyMissionUIDialog _instance;
    public static DailyMissionUIDialog Instance
    {
        get
        {
            if (_instance == null)
            {
                CommonAssetLoaderManager.Instance.LoadUI(typeof(DailyMissionUIDialog));
            }

            return _instance;
        }
    }
    ======================================
    这种注意要把_instance置空
    protected override void OnDestroy()
    {
        base.OnDestroy();
        Instance = null;
    }
    ======================================
    2.
    对于通用的图集  类似于IconManager
    通过调用SpriteAtlasLoadManager 来调用加载与卸载   通过传入界面Hash值   来控制SpriteAtlas是否卸载
    由于Unity内部的Atlas机制   SpriteAtlasManagerListener中会加载一次
    所以可能会出现atlas的bundle引用为1 卸载不掉（当然这里可以不采用引用计数+1） 目前在检查工具中会存在这种现象
    但是内存中是可以卸载掉的
    
    3.对于UI   我们都继承 AssetsLoader  如果想要加载使用它的接口LoadAsset
    他在Destroy的时候会卸掉加载的资源
    
    4.如果是单例或者不继承MonoBehavior的类里  如过想要卸载加载的东西  尽量避免各个不同模块使用相同接口
    请使用    
    加载：AssetsLoaderManager.Instance.LoadAsset
    卸载：UnLoadAll
    
    如果类似BossBattle  想要loading需要的资源而中途又可以停止加载资源  
    可以在 OnAssetLoadReceive 中使用 CacheLoad
    
    5.目前采用引用计数  来卸载AssetBundle   所以加了多少次 理论上  应该对应减多少次
    
    6.对于实例化物体  应使用PrefabLoadManager 去实例化  可参看PrefabLoadManager注释
    
    7.即使使用Bundle.Unload(ture)   并不会清理内存  搭配使用Resources.UnloadUnusedAssets();
      当缓存资源较多的时候，使用该接口会很慢（Iphone6）   所以根据策略进行使用
       
    8.使用完的对象池记得删除掉
    
    9.增加 FAST_UNITY_EDITOR 宏变量   打开该宏变量  会缓存加载的资源  为测试节省时间  
    如果想要使用客户端观察内存  请禁用该宏变量
*/

namespace Nireus
{
    public delegate void AssetBundleLoadCallBack(AssetBundle ab);
    
    public class AssetBundleObject
    {
        public string ab_name = string.Empty;
        public int ref_count = 0;
        public List<AssetBundleLoadCallBack> call_func_list = new List<AssetBundleLoadCallBack>();
        public AssetBundleCreateRequest request = null;
        public AssetBundle ab = null;
        public int depend_loading_count = 0;
        public List<AssetBundleObject> depends = new List<AssetBundleObject>();
        public bool loaded = false;
    }
    public partial class AssetManager : SingletonBehaviour<AssetManager>
    {
        AssetLoader[] loaders;
        const int MAX_THREAD = 4;
        
        void Awake()
        {
            loaders = new AssetLoader[MAX_THREAD];
            for (int i = 0; i < MAX_THREAD; i++)
            {
                loaders[i] = this.gameObject.AddComponent<AssetLoader>();
            }
        }

        public void load(IAssetLoaderReceiver receiver, string assetPath, object info = null, AssetLoadType loadType = AssetLoadType.COMMON, int threadID = 0, Type assetType = null)
        {
            loaders[0].load(receiver, assetPath, info, loadType, assetType);
        }

        /// <summary>
        /// 释放这个资源的整合bundle
        /// </summary>
        /// <param name="assetPath">资源原始路径</param>
        public void UnloadAsset(string assetPath, bool check = true, bool unloadAllLoadedObjects = false)
        {
#if !ASSET_RESOURCES
            loaders[0].Unload(assetPath, unloadAllLoadedObjects);
#endif
        }

        public void UnloadAssetDeferred(string assetPath, bool releaseAsset = false)
        {
#if !ASSET_RESOURCES
            loaders[0].UnloadAssetBundleDeferred(assetPath, releaseAsset);
#endif
        }
        public AssetBundle LoadAB(string assetPath, int threadID = 0)
        {
            Nireus.AssetBundleObject obj = loaders[threadID].LoadAB(assetPath);
            return obj.ab;
        }
        public void load(AssetLoadInfo loadInfo, int threadID = 0)
        {
            loaders[0].load(loadInfo);
        }

        public void loadAsync(List<AssetLoadInfo> loadInfo, int threadID = 0)
        {
            loaders[0].loadAsync(loadInfo);
        }

		public void loadAsync<T>(string assetPath, int threadID = 0, IAssetLoaderReceiver receiver = null) where T : UnityEngine.Object
		{
            loaders[0].loadAsync<T>(assetPath, receiver);
		}

		public void StopLoad(int threadID = 0)
        {
            loaders[threadID].StopLoadAsync();
        }
        public UnityEngine.Object loadSync(string assetPath, int threadID = 0)
        {
            return loaders[0].loadSync(assetPath);
        }

        public T loadSync<T>(string assetPath, int threadID = 0) where T : UnityEngine.Object
        {
            return loaders[0].loadSync<T>(assetPath);
        }

        public UnityEngine.Object loadSync(AssetLoadInfo load_info, int threadID = 0)
        {
            return loaders[0].loadSync(load_info);
        }

        public T Instantiate<T>(string assetPath) where T : UnityEngine.Object
        {
            return loaders[0].Instantiate<T>(assetPath);
        }
        public Sprite GetSprite(string assetPath)
        {
            return loaders[0].GetSprite(assetPath);
        }

        public void UnloadSceneAsync(string sceneName)
        {
            loaders[0].UnloadSceneAsync(sceneName);
        }

        public Dictionary<string, AssetBundleObject> GetAssetBundleObjectDic(int threadID = 0)
        {
            return loaders[0].GetBundleMap();
        }
    }

    public class AssetLoader : MonoBehaviour// : SingletonBehaviour<AssetManager>
    {
        #if UNITY_EDITOR
        private static Dictionary<string, UnityEngine.Object> _asset_data_map;
        #endif
#if !ASSET_RESOURCES
        [ShowInInspector]
        private Dictionary<string, AssetBundleObject> _asset_bundle_map = new Dictionary<string, AssetBundleObject>();
        private static List<string> assetBundleInLoading = new List<string>();
        private static HashSet<string> assetBundleUnloading = new HashSet<string>();
        private static List<AssetLoadInfo> waitingAssetBundleAssetsList = new List<AssetLoadInfo>();
#endif
        private Deque<AssetLoadInfo> _load_info_deque;
        private object _lock_obj = new object();
        private bool _startLoad = false;
        private IEnumerator _enumerator;
        public bool log { get; set; } = false;
        
        void Awake()
        {
#if FAST_UNITY_EDITOR
            _asset_data_map = new Dictionary<string, UnityEngine.Object>();
#endif
            _load_info_deque = new Deque<AssetLoadInfo>();
        }

        public Dictionary<string, AssetBundleObject> GetBundleMap()
        {
            return _asset_bundle_map;
        }
        public void load(IAssetLoaderReceiver receiver, string assetPath, object info = null, AssetLoadType loadType = AssetLoadType.COMMON, Type assetType = null)
        {
            this.load(Spawn(assetPath, info, receiver, loadType, assetType));
        }

        //↓↓↓↓↓↓↓↓↓↓↓↓↓↓异步版本↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓
        public Coroutine loadAsync(List<AssetLoadInfo> load_info_list)
        {
            _enumerator = startLoadList(load_info_list);
            return StartCoroutine(_enumerator);
        }

		public Coroutine loadAsync<T>(string assetPath, IAssetLoaderReceiver receiver) where T : UnityEngine.Object
		{
			var loadInfo = Spawn(assetPath, null, receiver, AssetLoadType.COMMON, typeof(T));
			List<AssetLoadInfo> load_info_list = new List<AssetLoadInfo> {
				loadInfo
			};
			return loadAsync(load_info_list);
		}

		public void StopLoadAsync()
        {
            if (_enumerator != null)
            {
                StopCoroutine(_enumerator);
            }
        }
        
#if !ASSET_RESOURCES
        private Dictionary<String, AssetBundleObject> _asset_bundle_loading_dic = new Dictionary<String, AssetBundleObject>();
        
#endif

        IEnumerator startLoadList(List<AssetLoadInfo> load_info_list)
        {
            yield return new WaitForEndOfFrame();

            Action<AssetLoadInfo, UnityEngine.Object> on_load = (AssetLoadInfo load_info, UnityEngine.Object asset_data) =>
            {
                load_info.receiver.OnAssetLoadProgress(load_info.assetPath, load_info.info, 1.0f, 1, 1);
                load_info.receiver.OnAssetLoadReceive(load_info.assetPath, asset_data, load_info.info);
                Despawn(load_info);
            };

            foreach (var load_info in load_info_list)
            {
#if ASSET_BUNDLE
                int max_deque_count = load_info_list.Count;
                yield return StartCoroutine(LoadAsync(load_info, max_deque_count, on_load));
#elif ASSET_RESOURCES
                yield return new WaitForEndOfFrame();
                var resReq = LoadFromResourcesAsync(load_info);
                yield return resReq;
                var asset_data = resReq.asset;

                AddAssetObjectToMap(load_info, asset_data);
                on_load(load_info, asset_data);
#else
                yield return new WaitForEndOfFrame();
                UnityEngine.Object asset_data = null;
#if FAST_UNITY_EDITOR
                if (_asset_data_map.TryGetValue(load_info.assetPath, out asset_data) == false ||
                    asset_data == null)
                       {
                    Type assetObjectType = load_info.assetObjectType;
                    if (assetObjectType == null) assetObjectType = typeof(UnityEngine.Object);

                    asset_data = UnityEditor.AssetDatabase.LoadAssetAtPath(load_info.assetPath, assetObjectType);
                    AddAssetObjectToMap(load_info, asset_data);
                }
#endif

                on_load(load_info, asset_data);
#endif
            }
        }

#if !ASSET_RESOURCES
        IEnumerator LoadAsync(AssetLoadInfo load_info, int max_deque_count, Action<AssetLoadInfo, UnityEngine.Object> on_load)
        {
            AssetBundleObject asset_data = null;
            string asset_bundle_name = FilePathHelper.AssetBundlePathToName(load_info.assetPath);
            if (_asset_bundle_map.TryGetValue(asset_bundle_name, out asset_data) == false)
            {
                if (load_info.type == AssetLoadType.SCENE)
                {
                    yield return StartCoroutine(LoadGameScene(load_info, max_deque_count));
                }
                else
                {
                    //要加载AssetData，先加载AssetBundle
                    AssetBundleObject asset_bundle = null;
                    //string asset_bundle_name = FilePathHelper.AssetBundlePathToName(load_info.assetPath);
                    assetBundleUnloading.Remove(asset_bundle_name);

                    //判断AssetBundle是否已经在加载，如果还在加载，则等待其加载完毕，再进行后续处理
                    AssetBundleObject create_request = null;
                    if (_asset_bundle_loading_dic.TryGetValue(asset_bundle_name, out create_request))
                    {
                        if (create_request.request == null)
                        {
                            GameDebug.LogError("create_request.request is null");
                        }
                        while (create_request.request.isDone == false)
                        {
                            yield return new WaitForEndOfFrame();
                        }
                        asset_bundle = create_request;
                    }

                    //判断AssetBundle是否已经加载完成
                    if (asset_bundle == null)
                    {
                        if (_asset_bundle_map.TryGetValue(asset_bundle_name, out asset_bundle) == false)
                        {
                            yield return LoadABAsync(load_info, asset_bundle_name, (AssetBundleObject loaded_asset_bundle) => {
                                asset_bundle = loaded_asset_bundle;
                            });
                        }
                    }

                    //失败
                    if (asset_bundle == null)
                    {
                        yield break;
                    }
                    //GameDebug.Log("bundle");

                    //从AssetBundle中加载Asset
                    AssetBundleRequest request = null;

                    request = asset_bundle.ab.LoadAssetAsync(load_info.assetPath.ToLower());
                    request.priority = (int)UnityEngine.ThreadPriority.Low;
                    yield return request;

                    asset_data = asset_bundle;
                    
                    on_load(load_info, asset_data.ab.LoadAsset(load_info.assetPath.ToLower()));
                    //GameDebug.Log("asset");

                    //保存Asset
                    //AddAssetObjectToMap(load_info, asset_data);
                    //on_load(load_info, request.asset);
                }
            }
            else
            {
                //引用计数+1
                _AddRef(asset_data);
                //asset_data = request.asset;
                on_load(load_info, asset_data.ab.LoadAsset(load_info.assetPath.ToLower()));
            }
            
        }

        IEnumerator LoadABAsync(AssetLoadInfo load_info, string asset_bundle_name, Action<AssetBundleObject> on_load)
        {
            if (m_Manifest == null)
            {
                if (!LoadManifest())
                {
                    yield break;
                }
            }

            //加载依赖资源包
            string asset_bunle_path = asset_bundle_name.Replace('_', '/');
            //找到依赖并且加载
            string[] dependencies = m_Manifest.GetAllDependencies(asset_bunle_path);
            for (int i = 0; i < dependencies.Length; i++)
            {
                string bundle_name_depend = dependencies[i].Replace('/', '_').ToLower();
                //判断AssetBundle是否已经被加载
                if (_asset_bundle_map.ContainsKey(bundle_name_depend))
                {
                    continue;
                }
                //判断AssetBundle是否正在被加载，如果正在被加载，则等待其加载完毕
                AssetBundleObject create_request = null;
                if (_asset_bundle_loading_dic.TryGetValue(bundle_name_depend, out create_request))
                {
                    if (create_request.request == null)
                    {
                        GameDebug.LogError("create_request.request is null");
                    }
                    while (create_request.request.isDone == false)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                    continue;
                }

                string path = FilePathHelper.GetFilePathAbsolute(bundle_name_depend);
#if UNITY_EDITOR
                GameDebug.Log("[LoadAssetBundle] dependency loaded: " + path + "\n name : " + bundle_name_depend);
#endif
                create_request = new AssetBundleObject();
                create_request.ab_name = bundle_name_depend;
                //依赖的计数再最后+1
                create_request.ref_count = 0;
                create_request.request = AssetBundle.LoadFromFileAsync(path);
                _asset_bundle_loading_dic.Add(bundle_name_depend, create_request);
                //GameGameDebug.Log("dep");
                yield return create_request;
                AssetBundle depend_asset_bundle = create_request.request.assetBundle;
                create_request.ab = depend_asset_bundle;
                _asset_bundle_map.Add(bundle_name_depend, create_request);
                _asset_bundle_loading_dic.Remove(bundle_name_depend);
            }


            //前置AssetBundle都加载完毕了，开始加载目标AssetBundle
            AssetBundleObject asset_bundle = null;
            if (_asset_bundle_map.TryGetValue(asset_bundle_name, out asset_bundle) == false)
            {
                string file_path = FilePathHelper.GetFilePathAbsolute(asset_bundle_name);

                if (String.IsNullOrEmpty(file_path) == false)
                {
#if UNITY_EDITOR
                    GameDebug.Log("AssetManager::LoadAB --> ab :" + asset_bundle_name + "\n asset path: " + load_info.assetPath + "\nfile path:" + file_path);
#endif
                    AssetBundleObject create_request = null;
                    if (_asset_bundle_loading_dic.TryGetValue(asset_bundle_name, out create_request))
                    {
                        if (create_request.request == null)
                        {
                            GameDebug.LogError("create_request.request is null");
                        }
                        while (create_request.request.isDone == false)
                        {
                            yield return new WaitForEndOfFrame();
                        }
                        asset_bundle = create_request;
                    }
                    else
                    {
                        create_request = new AssetBundleObject();
                        create_request.ab_name = asset_bundle_name;
                        create_request.ref_count = 0;
                        create_request.request = AssetBundle.LoadFromFileAsync(file_path);
       
                        _asset_bundle_loading_dic.Add(asset_bundle_name, create_request);
                        yield return create_request;
                        asset_bundle = create_request;
                        create_request.ab = asset_bundle.request.assetBundle;
                        
                        AddDependenceAB(asset_bundle_name,ref create_request);
                        //把依赖的bundle 引用计数+1
                        _AddRef(create_request);

                        _asset_bundle_map.Add(asset_bundle_name, create_request);
                        _asset_bundle_loading_dic.Remove(asset_bundle_name);
                    }
                }
            }
            else
            {
                _AddRef(asset_bundle);
            }
            if (asset_bundle.ab == null)
            {
                GameDebug.Log(asset_bundle.ab + " is null");
            }
            on_load(asset_bundle);
        }
#endif
        //↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑异步版本↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑

        // private void AddDependenceRef(string ab_name)
        // {
        //     string asset_bunle_path = ab_name.Replace('_', '/');
        //     string[] dependencies = m_Manifest.GetAllDependencies(asset_bunle_path);
        //     for (int i = 0; i < dependencies.Length; i++)
        //     {
        //         string bundle_name_depend = dependencies[i].Replace('/', '_').ToLower();
        //         if (!_asset_bundle_map.ContainsKey(bundle_name_depend))
        //         {
        //             GameDebug.Log("Dont have assetbundle " + bundle_name_depend);
        //             continue;
        //         }
        //         _AddRef(_asset_bundle_map[bundle_name_depend]);
        //     }
        // }
        private void AddDependenceAB(string ab_name,ref AssetBundleObject abObj)
        {
            string asset_bunle_path = ab_name.Replace('_', '/');
            string[] dependencies = m_Manifest.GetAllDependencies(asset_bunle_path);
            for (int i = 0; i < dependencies.Length; i++)
            {
                string bundle_name_depend = dependencies[i].Replace('/', '_').ToLower();
                if (_asset_bundle_map.TryGetValue(bundle_name_depend, out AssetBundleObject obj))
                {
                    abObj.depends.Add(obj);
                }
                else
                {
                    GameDebug.LogError(bundle_name_depend + "is not exit in _asset_bundle_map");
                }
            }
        }
        public void load(AssetLoadInfo loadInfo)
        {
            _load_info_deque.push(loadInfo);
            if (_load_info_deque.Count == 1)
            {
                //lock(_lock_obj)
                {
                    if (!_startLoad)
                    {
                        _startLoad = true;
                        StartCoroutine(startLoad());
                    }
                }
            }
        }
        private void _AddRef(AssetBundleObject ab_obj, bool load_depend_sync = false)
        {
            // if (ab_obj.ab_name == "assets_res_bundleres_prefabs_effectprefabs_bossbattle_bossbattleeffect.ab")
            // {
            //     GameDebug.Log("enen");
            // }
            ab_obj.ref_count++;

            if (log) GameDebug.Log($"[Bundle] [LoadManager] Ref Count Increase To [{ab_obj.ref_count}]: [{ab_obj.ab_name}]");

            foreach (var dp_object in ab_obj.depends)
            {
                _AddRef(dp_object, load_depend_sync: false);
            }
        }
        void Update()
        {
#if !ASSET_RESOURCES
            if (waitingAssetBundleAssetsList.Count > 0)
            {
                List<AssetLoadInfo> removeKey = new List<AssetLoadInfo>();
                foreach (var loadInfo in waitingAssetBundleAssetsList)
                {
                    string asset_bundle_name = FilePathHelper.AssetBundlePathToName(loadInfo.assetPath);

                    if (assetBundleInLoading.Contains(asset_bundle_name) == false)
                    {
                        load(loadInfo);
                        removeKey.Add(loadInfo);
                    }
                }
                foreach (var item in removeKey)
                {
                    waitingAssetBundleAssetsList.Remove(item);

                }

            }
#endif
            HandleUnloadList();
        }
        //创建临时存储变量，用于提升性能
        private List<string> _temp = new List<string>();

        private void HandleUnloadList()
        {
            if (assetBundleUnloading.Count == 0)
            {
                return;
            }
            _temp.Clear();
            foreach (var unloadName in assetBundleUnloading)
            {
                AssetBundleObject ab_obj = _asset_bundle_map[unloadName];
                //外部调用销毁，然后又瞬间调用加载，有可能导致【引用大于0】，所以此处要判断引用计数
                //只有引用计数小于0的，才能执行销毁，否则，直接将它从销毁队列中移除即可（不执行销毁）
                if (ab_obj.ref_count <= 0)
                {
                    //如果尚未加载完，暂时不处理，让他继续呆在销毁队列中，等加载完再执行销毁
                    //如果不这么做，就会造成 [伪释放]，释放了一个null，并没有打断后台的AB加载，AB最终会留在内存
                    if (ab_obj.ab == null)
                    {
                        continue;
                    }
                    if (ab_obj.ab != null)
                    {
                        ab_obj.ab.Unload(true);
                        ab_obj.ab = null;
                    }
                    if (log) GameDebug.LogError($"[Bundle] [LoadManager] Unload: [{ab_obj.ab_name}]");
                    _asset_bundle_map.Remove(ab_obj.ab_name);
                }
                _temp.Add(unloadName);
            }
            foreach (var abObj in _temp)
            {
                assetBundleUnloading.Remove(abObj);
            }
        }
        IEnumerator LoadGameScene(AssetLoadInfo load_info, int max_deque_count)
        {
            UnityEngine.AsyncOperation scene_load_ao =
                           UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(load_info.assetPath,
                           UnityEngine.SceneManagement.LoadSceneMode.Additive);
            while (scene_load_ao.isDone == false)
            {
                yield return null;
                load_info.receiver.OnAssetLoadProgress(load_info.assetPath, load_info.info, scene_load_ao.progress / max_deque_count, 1, 1);
            }
            // yield return scene_load_ao;
            //_asset_data_map.Add(load_info.assetPath, new UnityEngine.Object());
            yield return new WaitForSeconds(0.1f);

        }

#if !ASSET_RESOURCES
        //Bundle配置文件
        AssetBundleManifest m_Manifest;
        public bool LoadManifest()
        {
            try
            {
                string path = FilePathHelper.GetManifestAssetBundleFullPath();
                GameDebug.Log("Manifest：  " + path);
                AssetBundle ab = AssetBundle.LoadFromFile(path);
                if (ab == null)
                {
                    GameDebug.LogError("Not found Manifest :" + path);
                    return false;
                }
                m_Manifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                if (m_Manifest == null)
                {
                    GameDebug.LogError("There is no AssetBundleManifest in file StreamingAssets");
                    return false;
                }
                ab.Unload(false);
            }
            catch (System.Exception e)
            {
                return false;
            }
            return true;
        }

        public AssetBundleObject LoadAB(string asset_bundle_name)
        {

            if (m_Manifest == null)
            {
                if (!LoadManifest())
                {
                    return null;
                }
            }

            //加载依赖资源包
            string asset_bunle_path = asset_bundle_name.Replace('_', '/');
            string[] dependencies = m_Manifest.GetAllDependencies(asset_bunle_path);//找到依赖并且加载
            for (int i = 0; i < dependencies.Length; i++)
            {
                string bundle_name_tmp = dependencies[i].Replace('/', '_').ToLower(); ;
                if (_asset_bundle_map.ContainsKey(bundle_name_tmp)) continue;
                string path = FilePathHelper.GetFilePathAbsolute(bundle_name_tmp);
#if UNITY_EDITOR
                GameDebug.Log("[LoadAssetBundle] dependency loaded: " + path + "\n name : " + bundle_name_tmp);
#endif
                AssetBundle temp = AssetBundle.LoadFromFile(path);
                AssetBundleObject abObj = new AssetBundleObject();
                abObj.ab = temp;
                abObj.ab_name = bundle_name_tmp;
                //加载最后计数+1
                abObj.ref_count = 0;
                if (temp == null)
                {
                    GameDebug.LogError("There is no target bundle in this path: " + path);
                    return null;
                }
                _asset_bundle_map.Add(bundle_name_tmp, abObj);
            }

            AssetBundleObject assetBundleObj = null;
            string file_path = FilePathHelper.GetFilePathAbsolute(asset_bundle_name);
            if (file_path != "")
            {
                assetBundleInLoading.Add(asset_bundle_name);
#if UNITY_EDITOR
                GameDebug.Log("AssetManager::startLoad --> ab :" + asset_bundle_name + "\n asset path: " + asset_bunle_path + "\nfile path:" + file_path);
#endif
                var assetBundle = AssetBundle.LoadFromFile(file_path);

                if (_asset_bundle_map.ContainsKey(asset_bundle_name))
                {
                    assetBundleObj = _asset_bundle_map[asset_bundle_name];
                    //把加载计数+1
                    _AddRef(assetBundleObj);
                    //Debug.LogErrorFormat("AssetManager::startLoad --> asset_bundle_map.ContainsKey,asset_bundle_name={0}", asset_bundle_name);
                }
                else if (assetBundle != null)
                {
                    AssetBundleObject abObj = new AssetBundleObject();
                    abObj.ab = assetBundle;
                    abObj.ab_name = asset_bundle_name;
                    abObj.ref_count = 0;
                    _asset_bundle_map.Add(asset_bundle_name, abObj);
                    assetBundleObj = abObj;
                    
                    AddDependenceAB(asset_bundle_name,ref abObj);
                    _AddRef(abObj);
                    
                }
                assetBundleInLoading.Remove(asset_bundle_name);
            }
            return assetBundleObj;
            //callback(assetBundle);
            //yield return new WaitForEndOfFrame();

        }
#endif

        void AddAssetObjectToMap(AssetLoadInfo load_info, UnityEngine.Object asset_data)
        {
#if FAST_UNITY_EDITOR
            if (asset_data == null)
            {
                GameDebug.LogError("Asset Load Error:" + load_info.assetPath);
            }
            else
            {
                if (_asset_data_map.ContainsKey(load_info.assetPath) == false)
                {
                    _asset_data_map.Add(load_info.assetPath, asset_data);
                }
            }
#endif
        }

        IEnumerator startLoad()
        {
            yield return null;//new WaitForSeconds(0.1f);

            UnityEngine.Object asset_data = null;
            AssetBundleObject asset_data_obj = null;
            int max_deque_count = _load_info_deque.Count;
            while (_load_info_deque.Count > 0)
            {
                AssetLoadInfo load_info = _load_info_deque.shift();
                string asset_bundle_name = FilePathHelper.AssetBundlePathToName(load_info.assetPath);
                if (_asset_bundle_map.TryGetValue(asset_bundle_name, out asset_data_obj) == false)
                {
                    if (load_info.type == AssetLoadType.SCENE)
                    {
                        yield return StartCoroutine(LoadGameScene(load_info, max_deque_count));
                    }
                    else
                    {

#if ASSET_BUNDLE
                        //string asset_bundle_name = FilePathHelper.AssetBundlePathToName(load_info.assetPath);

                        if (assetBundleInLoading.Contains(asset_bundle_name))
                        {
                            waitingAssetBundleAssetsList.Add(load_info);
                            //  yield return new WaitForSeconds(1f);// WaitWhile(() => assetBundleInLoading.Contains(asset_bundle_name) == false);
                        }
                        else
                        {
                            AssetBundleObject assetBundle = null;

                            if (_asset_bundle_map.TryGetValue(asset_bundle_name, out assetBundle) == false)
                            {
                                assetBundle = (LoadAB(asset_bundle_name));
                            }

                            if (assetBundle != null)
                            {
                                if (load_info.assetObjectType != null)
                                {
                                    asset_data = assetBundle.ab.LoadAsset(load_info.assetPath.ToLower(), load_info.assetObjectType);
                                }
                                else
                                {
                                    asset_data = assetBundle.ab.LoadAsset(load_info.assetPath.ToLower());
                                }
                                yield return new WaitForEndOfFrame();
                            }
                            //AddAssetObjectToMap(load_info, asset_data);
                        }
#elif ASSET_RESOURCES
                        var resReq = LoadFromResourcesAsync(load_info);
                        yield return resReq;
                        asset_data = resReq.asset;
                        AddAssetObjectToMap(load_info, asset_data);
                        yield return null;
#else
#if FAST_UNITY_EDITOR
                        if (_asset_data_map.TryGetValue(load_info.assetPath, out asset_data) == false ||
                            asset_data == null)
#endif
                        {
                            //Type assetObjectType = load_info.assetObjectType;
                            //if (assetObjectType == null) assetObjectType = typeof(UnityEngine.Object);
                            //asset_data = UnityEditor.AssetDatabase.LoadAssetAtPath(load_info.assetPath, assetObjectType);
                            //AddAssetObjectToMap(load_info, asset_data);
                            //yield return null;
                        }
#endif
                    }
                }
                else
                {
                    if (load_info.assetObjectType != null)
                    {
                        asset_data = asset_data_obj.ab.LoadAsset(load_info.assetPath.ToLower(), load_info.assetObjectType);
                    }
                    else
                    {
                        asset_data = asset_data_obj.ab.LoadAsset(load_info.assetPath.ToLower());
                    }
                    _AddRef(asset_data_obj);
                }
                load_info.receiver.OnAssetLoadProgress(load_info.assetPath, load_info.info, 1.0f, 1, 1);
                load_info.receiver.OnAssetLoadReceive(load_info.assetPath, asset_data, load_info.info);
                Despawn(load_info);
            }
            //lock (_lock_obj)
            {
                _startLoad = false;
            }
        }


        public UnityEngine.Object loadSync(string assetPath)
        {
            var loadInfo = Spawn(assetPath, null, null, AssetLoadType.COMMON);
            UnityEngine.Object o = loadSync(loadInfo);
            Despawn(loadInfo);
            return o;
        }

        public T loadSync<T>(string assetPath) where T : UnityEngine.Object
        {
            var loadInfo = Spawn(assetPath, null, null, AssetLoadType.COMMON, typeof(T));
            UnityEngine.Object o = loadSync(loadInfo);
            Despawn(loadInfo);
            return o as T;
        }

        public UnityEngine.Object loadSync(AssetLoadInfo load_info)
        {
            AssetBundleObject asset_data_obj = null;
            UnityEngine.Object asset_data = null;
            string asset_bundle_name = FilePathHelper.AssetBundlePathToName(load_info.assetPath);
            if (_asset_bundle_map.TryGetValue(asset_bundle_name, out asset_data_obj) == false || asset_data_obj==null)
            {
#if ASSET_BUNDLE
                assetBundleUnloading.Remove(asset_bundle_name);
                AssetBundleObject assetBundle = null;
                if (_asset_bundle_map.TryGetValue(asset_bundle_name, out assetBundle) == false)
                {
                    if (_asset_bundle_map.TryGetValue(asset_bundle_name, out assetBundle) == false)
                    {
                        assetBundle = (LoadAB( asset_bundle_name));
                    }
                    else
                    {
                        _AddRef(assetBundle);
                    }
                }
                else
                {
                    _AddRef(assetBundle);
                }

                if (assetBundle != null)
                {
                    if (load_info.assetObjectType != null)
                    {
                        asset_data = assetBundle.ab.LoadAsset(load_info.assetPath.ToLower(), load_info.assetObjectType);
                    }
                    else
                    {
                        asset_data = assetBundle.ab.LoadAsset(load_info.assetPath.ToLower());
                    }
                }

#elif ASSET_RESOURCES
                asset_data = LoadFromResourcesSync(load_info);

#else
#if FAST_UNITY_EDITOR
                if (_asset_data_map.TryGetValue(load_info.assetPath, out asset_data) == false || asset_data == null)
                  {
                    Type assetObjectType = load_info.assetObjectType;
                    if (assetObjectType == null) assetObjectType = typeof(UnityEngine.Object);
                    asset_data = UnityEditor.AssetDatabase.LoadAssetAtPath(load_info.assetPath, assetObjectType);
                    AddAssetObjectToMap(load_info,asset_data);
                }
#endif

#endif
                if (asset_data == null)
                {
                    GameDebug.LogError("Asset Load Error:" + load_info.assetPath);
                }
                else
                {
                    // if (load_info.type == AssetLoadType.UI)
                    // {
                    //     var newGo = GameObject.Instantiate(asset_data) as GameObject;
                    //     if (newGo.GetComponent((Type)load_info.info) == null)
                    //         newGo.AddComponent((Type)load_info.info);
                    //     (newGo.transform as RectTransform).anchorMax = new Vector2(0.5f, 0.5f);
                    //     (newGo.transform as RectTransform).anchorMin = new Vector2(0.5f, 0.5f);
                    //     newGo.SetActive(false);
                    //     asset_data = newGo;
                    // }
                    // if (_asset_data_map.ContainsKey(load_info.assetPath))
                    // {
                    //     Debug.LogErrorFormat("AssetManager::loadSync --> asset_data_map.ContainsKey,assetPath={0}", load_info.assetPath);
                    // }
                    // else
                    // {
                    //     _asset_data_map.Add(load_info.assetPath, asset_data);
                    // }
                }
            }
            else
            {
                _AddRef(asset_data_obj);
                if (asset_data_obj != null)
                {
                    if (load_info.assetObjectType != null)
                    {
                        asset_data = asset_data_obj.ab.LoadAsset(load_info.assetPath.ToLower(), load_info.assetObjectType);
                    }
                    else
                    {
                        asset_data = asset_data_obj.ab.LoadAsset(load_info.assetPath.ToLower());
                    }
                }
            }
            return asset_data;
        }

        public T Instantiate<T>(string assetPath) where T : UnityEngine.Object
        {
            T obj = loadSync<T>(assetPath);
            if (obj == null)
            {
                return null;
            }
            return GameObject.Instantiate<T>(obj);
        }
        public Sprite GetSprite(string assetPath)
        {
            Texture2D tex = loadSync<Texture2D>(assetPath);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        public void UnloadSceneAsync(string sceneName)
        {
            //_asset_data_map.Remove(sceneName);
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
        }

#if !ASSET_RESOURCES
        public void Unload(string path, bool unloadAllLoadedObjects = false)
        {
            string asset_bunle_name = FilePathHelper.AssetBundlePathToName(path);
            if (asset_bunle_name == "assets_res_bundleres_prefabs_fight.ab")
            {
                GameDebug.Log("en");
            }
            if (_asset_bundle_map.ContainsKey(asset_bunle_name))
            {
                AssetBundleObject ab_obj = _asset_bundle_map[asset_bunle_name];
                ab_obj.ref_count--;

                foreach (var dp_ab_object in ab_obj.depends)
                {
                    //string bundlePath = dp_ab_object.ab_name.Replace('_', '/');
                    Unload(dp_ab_object);
                }
                
                if (ab_obj.ref_count <= 0)
                {
                    if (ab_obj.ref_count < 0)
                    {
                        GameDebug.LogError($"[Bundle] [LoadManager] UnLoadAssetbundle ref_count error, asset_name: {asset_bunle_name}");
                    }
                    assetBundleUnloading.Add(ab_obj.ab_name);
                }
            }
            
        }

        private void Unload(AssetBundleObject ab_obj)
        {
            if(ab_obj == null) return;
            if (ab_obj.ab_name == "assets_res_bundleres_animations_fight_role_common_kongshou.ab")
            {
                GameDebug.Log("en");
            }
            ab_obj.ref_count--;
            foreach (var dp_ab_object in ab_obj.depends)
            {
                //string bundlePath = dp_ab_object.ab_name.Replace('_', '/');
                Unload(dp_ab_object);
            }
            if (ab_obj.ref_count <= 0)
            {
                if (ab_obj.ref_count < 0)
                {
                    GameDebug.LogError($"[Bundle] [LoadManager] UnLoadAssetbundle ref_count error, asset_name: {ab_obj.ab_name}");
                }
                assetBundleUnloading.Add(ab_obj.ab_name);
            }
        }
        
        private Coroutine _unloadAssetBundleRoutine;
        public void UnloadAssetBundleDeferred(string assetPath, bool releaseAsset = false)
        {
            string assetBundleName = FilePathHelper.AssetBundlePathToName(assetPath);
            assetBundleUnloading.Add(assetBundleName);

            // if (releaseAsset)
            // {
            //     _asset_data_map.Remove(assetPath);
            // }

            if (_unloadAssetBundleRoutine == null)
            {
                _unloadAssetBundleRoutine = StartCoroutine(UnloadAssetBundleRoutine());
            }
        }

        IEnumerator UnloadAssetBundleRoutine()
        {
            yield return null;
            yield return null;

            foreach (var bundleName in assetBundleUnloading)
            {
                if (_asset_bundle_map.TryGetValue(bundleName, out var assetBundle))
                {
                    assetBundle.ab.Unload(false);
                    _asset_bundle_map.Remove(bundleName);
                }
            }

            assetBundleUnloading.Clear();

            _unloadAssetBundleRoutine = null; 
        }

        public void UnloadAssetBundle(string assetPath, bool unloadAllLoadedObjects = false)
        {
            string asset_bunle_name = FilePathHelper.AssetBundlePathToName(assetPath);
            AssetBundleObject asset_bundle;

            /*
            if (check)
            {
                int count = 0;
                foreach (var kv in _asset_data_map)
                {
                    string asset_bunle_name_t = FilePathHelper.AssetBundlePathToName(kv.Key);
                    if (asset_bunle_name_t == assetPath)
                    {
                        count++;
                    }
                }

                if (count > 1)
                {
                    GameDebug.LogWarning("[AssetManager.UnloadAssetBundle] Failed: Multi assets is ");
                    throw new Exception("only release single asset bundle");
                }
            }
            */

            if (_asset_bundle_map.TryGetValue(asset_bunle_name, out asset_bundle))
            {
                asset_bundle.ab.Unload(unloadAllLoadedObjects);
                _asset_bundle_map.Remove(asset_bunle_name);
            }

            UnityEngine.Object asset_obj;
            // if (_asset_data_map.TryGetValue(assetPath, out asset_obj))
            // {
            //     _asset_data_map.Remove(assetPath);
            // }

        }
#endif

        private UnityEngine.Object LoadFromResourcesSync(AssetLoadInfo loadInfo)
        {
            Type assetObjectType = loadInfo.assetObjectType;
            if (assetObjectType == null) assetObjectType = typeof(UnityEngine.Object);
            var asset = UnityEngine.Resources.Load(_fixAssetPathForAssetInResources(loadInfo.assetPath), assetObjectType);
            return asset;
        }

        private ResourceRequest LoadFromResourcesAsync(AssetLoadInfo loadInfo)
        {
            Type assetObjectType = loadInfo.assetObjectType;
            if (assetObjectType == null) assetObjectType = typeof(UnityEngine.Object);
            return UnityEngine.Resources.LoadAsync(_fixAssetPathForAssetInResources(loadInfo.assetPath), assetObjectType);
        }

        private string _fixAssetPathForAssetInResources(string path)
        {
            var retPath = path;
            if (retPath.StartsWith("Assets/"))
            {
                retPath = retPath.Substring(7);
            }

            retPath = retPath.Remove(retPath.LastIndexOf('.'));

            return retPath;
        }

#region Pool
        private List<AssetLoadInfo> spawnedDatas = new List<AssetLoadInfo>();
        private List<AssetLoadInfo> despawnedDatas = new List<AssetLoadInfo>();

        private AssetLoadInfo CreateNewData()
        {
            var audioData = new AssetLoadInfo("", null, null);
            return audioData;
        }

        public AssetLoadInfo Spawn(string assetPath, object info, IAssetLoaderReceiver receiver, AssetLoadType assetType, Type objType = null)
        {
            AssetLoadInfo data = null;
            if (despawnedDatas.Count != 0)
            {
                data = despawnedDatas[despawnedDatas.Count - 1];
                despawnedDatas.RemoveAt(despawnedDatas.Count - 1);
            }
            else
            {
                data = CreateNewData();
            }
            spawnedDatas.Add(data);
            data.assetPath = assetPath;
            data.info = info;
            data.receiver = receiver;
            data.type = assetType;
            data.assetObjectType = objType;
            return data;
        }

        private void Despawn(AssetLoadInfo data)
        {
            if (spawnedDatas.Contains(data))
            {
                spawnedDatas.Remove(data);
                despawnedDatas.Add(data);
            }
        }

#endregion

    }


}
