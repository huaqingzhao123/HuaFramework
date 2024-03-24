/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/9/17 0:19:00
 */

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using XMLib;
using XMLib.AM;
using XMLib.DataHandlers;

namespace AliveCell
{
    public partial class GlobalSetting
    {
        [SerializeField]
        protected ResourceService.Setting _resource;

        public static ResourceService.Setting resource => Inst._resource;
    }

    /// <summary>
    /// ResourceDriver
    /// </summary>
    public class ResourceService : IServiceInitialize, IServiceLateInitialize, IDisposable
    {
        [Serializable]
        public class Setting
        {
            public List<string> preAMConfigs;
            public string configPath = "Data/ResourceInfo";
            public bool enableDebugger = false;
        }

        protected Setting setting => GlobalSetting.resource;

        protected readonly ObjectPool<int> pool;

        protected readonly Dictionary<int, GameObject> prefabDict;

        protected List<ResourceInfo> infos;
        protected readonly Dictionary<int, ResourceInfo> id2Info;
        protected readonly Dictionary<string, ResourceInfo> name2Info;

        protected Transform poolRoot;

        protected Dictionary<string, object> configCache;
        protected Dictionary<int, EnemyInfo> enemyInfoCache;
        protected Dictionary<int, PlayerInfo> playerInfoCache;

        public SuperLogHandler LogHandler = SuperLogHandler.Create("RS");

        public ResourceService()
        {
            infos = null;
            pool = new ObjectPool<int>();
            prefabDict = new Dictionary<int, GameObject>(32);
            id2Info = new Dictionary<int, ResourceInfo>(32);
            name2Info = new Dictionary<string, ResourceInfo>();
            configCache = new Dictionary<string, object>();
            enemyInfoCache = new Dictionary<int, EnemyInfo>();
            playerInfoCache = new Dictionary<int, PlayerInfo>();
        }

        public IEnumerator OnServiceInitialize()
        {
            DOTween.Init();

            //初始化
            ActionMachineHelper.Init(LoadMachineConfig);

            //
            InitializeConfigs();

            poolRoot = new GameObject("[AC]Res").transform;
            poolRoot.transform.position = Vector3.one * 10000;
            poolRoot.transform.localScale = Vector3.zero;
            GameObject.DontDestroyOnLoad(poolRoot.gameObject);

            if (setting.enableDebugger)
            {
                CreateGO(10000002);
            }

            yield break;
        }

        public IEnumerator OnServiceLateInitialize()
        {
            //yield return new WaitForSecondsRealtime(0.5f);
            yield return OnPreload();
        }

        private IEnumerator OnPreload()
        {
            using (var watcher = new TimeWatcher("Preload"))
            {
                int maxCnt = 15;
                int loadCnt = 0;
                //预加载预制体
                foreach (var item in infos)
                {
                    if (item.type == ResourceType.Prefab && item.preloadCnt > 0)
                    {
                        PreloadGOs(item.id, item.preloadCnt);
                        loadCnt += item.preloadCnt;
                    }
                    if (loadCnt > maxCnt)
                    {
                        loadCnt = 0;
                        yield return null;
                    }
                }

                //预加载配置文件
                foreach (var item in setting.preAMConfigs)
                {
                    ActionMachineHelper.GetMachineConfig(item);
                    yield return null;
                }
            }

            yield break;
        }

        public void PreloadGOs(int prefabId, int count)
        {
            for (int i = pool.Count(prefabId); i <= count; i++)
            {
                GameObject preObj = FindPrefab(prefabId);
                GameObject obj = GameObject.Instantiate(preObj);
                DestroyGO(obj);
            }
        }

        public void PreloadGOs(Dictionary<int, int> prefabCounts)
        {
            foreach (var item in prefabCounts)
            {
                PreloadGOs(item.Key, item.Value);
            }
        }

        public void Dispose()
        {
            pool.Clear();
            if (null != poolRoot)
            {
                GameObject.Destroy(poolRoot.gameObject);
                poolRoot = null;
            }
        }

        public ResourceInfo FindInfo(int id)
        {
            return id2Info.TryGetValue(id, out ResourceInfo info) ? info : null;
        }

        public string FindPath(int id)
        {
            return FindInfo(id)?.path;
        }

        public GameObject FindPrefab(int prefabID)
        {
            GameObject prefab = null;

            if (prefabDict.TryGetValue(prefabID, out prefab))
            {
                return prefab;
            }

            string resourcePath = FindPath(prefabID);
            prefab = Resources.Load<GameObject>(resourcePath);
            if (null == prefab)
            {
                return null;
            }

            prefabDict.Add(prefabID, prefab);

            return prefab;
        }

        public GameObject CreateGO(int prefabID)
        {
            GameObject result = null;
            IResourceItem item = pool.Pop<IResourceItem>(prefabID);
            if (null != item)
            {
                item.transform.SetParent(null, false);
                SceneManager.MoveGameObjectToScene(item.gameObject, SceneManager.GetActiveScene());

                if (item.useActive)
                {
                    item.gameObject.SetActive(true);
                }

                result = item.gameObject;
            }
            else
            {
                GameObject prefab = FindPrefab(prefabID);
                if (null == prefab)
                {
                    throw new RuntimeException($"未找到 ID:{prefabID} 预制");
                }

                result = GameObject.Instantiate(prefab);
                result.name = prefab.name;
            }

            return result;
        }

        public void DestroyGO(GameObject obj)
        {
            if (null == obj)
            {
                return;
            }

            IResourceItem item = obj.GetComponent<IResourceItem>();
            DestroyGO(item);
        }

        public void DestroyGO<T>(T item) where T : IResourceItem
        {
            if (null == item
                || (item is UnityEngine.Object obj && obj == null))
            {
                return;
            }

            if (item.forceDestroy)
            {
                GameObject.Destroy(item.gameObject);
                return;
            }

            if (item.useActive)
            {
                item.gameObject.SetActive(false);
            }
            item.transform.SetParent(poolRoot, false);
            pool.Push(item);
        }

        public T Load<T>(string name) where T : UnityEngine.Object
        {
            if (!name2Info.TryGetValue(name, out ResourceInfo info))
            {
                throw new RuntimeException($"未找到 [{name}] 资源信息");
            }

            T asset = Resources.Load<T>(info.path);
            if (asset == null)
            {
                throw new RuntimeException($"[{name}] 资源加载失败");
            }

            return asset;
        }

        public List<T> LoadConfig<T>(string childName = null)
        {
            string fileName = DataHandler.GetFileName<T>(childName);
            if (!configCache.TryGetValue(fileName, out object config))
            {
                TextAsset asset = Load<TextAsset>(fileName);
                config = DataUtility.FromJson<List<T>>(asset.text);
                configCache[fileName] = config;
            }
            return (List<T>)config;
        }

        #region config

        public void InitializeConfigs()
        {
            infos = LoadResourceInfo();
            foreach (var info in infos)
            {
                if (info.id != 0)
                {
                    id2Info.Add(info.id, info);
                }
                name2Info.Add(info.name, info);
            }
            //

            List<EnemyInfo> enemyInfos = LoadConfig<EnemyInfo>();
            foreach (var info in enemyInfos)
            {
                enemyInfoCache.Add(info.id, info);
            }

            List<PlayerInfo> playerInfos = LoadConfig<PlayerInfo>();
            foreach (var info in playerInfos)
            {
                playerInfoCache.Add(info.id, info);
            }
        }

        public List<ResourceInfo> LoadResourceInfo()
        {
            TextAsset asset = Resources.Load<TextAsset>(setting.configPath);
            List<ResourceInfo> infos = DataUtility.FromJson<List<ResourceInfo>>(asset.text);
            return infos;
        }

        public MachineConfig LoadMachineConfig(string name)
        {
            TextAsset asset = Load<TextAsset>(name);
            return DataUtility.FromJson<MachineConfig>(asset.text);
        }

        public EnemyInfo LoadEnemyInfo(int id)
        {
            return enemyInfoCache.TryGetValue(id, out var info) ? info : null;
        }

        public PlayerInfo LoadPlayerInfo(int id)
        {
            return playerInfoCache.TryGetValue(id, out var info) ? info : null;
        }

        #endregion config
    }
}