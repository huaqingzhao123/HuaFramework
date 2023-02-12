using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XMLib;

namespace AliveCell
{
    public partial class GlobalSetting
    {
        [SerializeField]
        private SceneService.Setting _scene = null;

        public static SceneService.Setting scene => Inst._scene;
    }

    /// <summary>
    /// 场景服务
    /// </summary>
    public class SceneService : IServiceLateInitialize, IDisposable, IMonoStart
    {
        [Serializable]
        public class SceneInfo
        {
            public string name;
            public int id;
            public GameMode mode;
            public string matchCode;
        }

        [Serializable]
        public class Setting
        {
            public List<SceneInfo> scenes;

            public string FromID(int id)
            {
                Initialize();
                if (!_id2Info.TryGetValue(id, out SceneInfo info))
                {
                    SuperLog.Log($"未找到 ID({id}) 对应场景");
                }

                return info.name;
            }

            public string MatchFromID(int id)
            {
                Initialize();
                if (!_id2Info.TryGetValue(id, out SceneInfo info))
                {
                    SuperLog.Log($"未找到 ID({id}) 对应场景的 MatchCode");
                }
                return info.matchCode;
            }

            public int GetID(string name)
            {
                Initialize();
                if (!_name2Id.TryGetValue(name, out int id))
                {
                    SuperLog.Log($"未找到 Name({name}) 对应场景的id");
                }
                return id;
            }

            public SceneInfo Get(int id)
            {
                Initialize();
                if (!_id2Info.TryGetValue(id, out SceneInfo info))
                {
                    SuperLog.Log($"未找到 ID({id}) 对应场景的 MatchCode");
                }

                return info;
            }

            public SceneInfo Get(string name)
            {
                Initialize();
                int id = GetID(name);
                return Get(id);
            }

            [NonSerialized] private Dictionary<string, int> _name2Id;
            [NonSerialized] private Dictionary<int, SceneInfo> _id2Info;
            [NonSerialized] private bool _isInited = false;

            public void Initialize()
            {
                if (_isInited)
                {
                    return;
                }
                _isInited = true;

                _name2Id = new Dictionary<string, int>();
                _id2Info = new Dictionary<int, SceneInfo>();
                foreach (var info in scenes)
                {
                    _name2Id[info.name] = info.id;
                    _id2Info[info.id] = info;
                }
            }
        }

        public Setting setting => GlobalSetting.scene;

        private Dictionary<Type, ISubScene> type2scene = new Dictionary<Type, ISubScene>();

        public SuperLogHandler LogHandler = SuperLogHandler.Create("SS");

        public Type lastSceneType { get; protected set; } = null;
        public Type currentSceneType { get; protected set; } = null;

        public SceneDisplayController displayController { get; private set; }

        public static class SceneNames
        {
            public const string Game = "Game";
        }

        public SceneService()
        {
            type2scene[typeof(GameScene)] = new GameScene(this);
            //type2scene[typeof(MainScene)] = new MainScene(this);

            displayController = new SceneDisplayController();
        }

        public SceneInfo GetInfo()
        {
            return setting.Get(1);
        }

        private T Get<T>() where T : class, ISubScene, new() => Get(typeof(T)) as T;

        private ISubScene Get(Type sceneType) => type2scene.TryGetValue(sceneType, out ISubScene target) ? target : null;

        public IEnumerator Load<T>(bool useTransition = false, params object[] objs) where T : class, ISubScene
        {
            yield return Load(typeof(T), useTransition, objs);
        }

        private IEnumerator Load(Type sceneType, bool useTransition = false, params object[] objs)
        {
            ISubScene subScene = null;
            if (currentSceneType != null)
            {
                subScene = Get(currentSceneType);
                App.Trigger(EventTypes.Scene_UnInitialize, subScene);
                yield return subScene.Unload();
            }

            lastSceneType = currentSceneType;
            currentSceneType = sceneType;

            subScene = Get(currentSceneType);
            yield return subScene.Load(objs);
            App.Trigger(EventTypes.Scene_Initialize, subScene);
        }

        public IEnumerator OnServiceLateInitialize()
        {
            yield return null;
        }

        public void OnMonoStart()
        {
            App.StartCoroutine(WaittingLoaded());
        }

        public void Dispose()
        {
            App.Off(this);
            displayController.Dispose();
        }

        private IEnumerator WaittingLoaded()
        {
            switch (App.launchMode)
            {
                case LaunchMode.Normal:
                    yield return OnLoadNormal();
                    break;

                case LaunchMode.HangUp:
                    yield return OnLoadHangUp();
                    break;
                case LaunchMode.Escort:
                    yield return OnLoadEscort();
                    break;
                case LaunchMode.PracticeSkill:
                    yield return OnLoadPracticeSkill();
                    break;
                case LaunchMode.Wanted:
                    yield return OnLoadWanted();
                    break;
                case LaunchMode.Tower:
                    yield return OnLoadTower();
                    break;
                case LaunchMode.Arena:
                    yield return OnLoadArena();
                    break;
            }
        }



        private IEnumerator OnLoadNormal()
        {
            float minWaitTime = 3f;
            float enterTime = Time.unscaledTime;
            yield return new WaitUntil(() => App.isInited);//等待初始化完成

            if (IInputProxyBase.IsActive)
            {
                //yield return Load<GameScene>(false, GameMode.Normal, GameLandManager.File_Time);
            }
            else
            {
                //yield return Load<GameScene>(false, GameMode.Replay, GameLandManager.File_Time);
            }

            yield return new WaitUntil(() => Time.unscaledTime - enterTime > minWaitTime);//控制最小时间
        }

        private IEnumerator OnLoadHangUp()
        {
            float minWaitTime = 3f;
            float enterTime = Time.unscaledTime;
            yield return new WaitUntil(() => App.isInited);//等待初始化完成

            //yield return Load<GameScene>(false, GameMode.HangUP, GameLandManager.File_Time);

            yield return new WaitUntil(() => Time.unscaledTime - enterTime > minWaitTime);//控制最小时间
        }

        private IEnumerator OnLoadEscort()
        {
            float minWaitTime = 3f;
            float enterTime = Time.unscaledTime;
            yield return new WaitUntil(() => App.isInited);//等待初始化完成

            //yield return Load<GameScene>(false, GameMode.Escort, GameLandManager.File_Time);

            yield return new WaitUntil(() => Time.unscaledTime - enterTime > minWaitTime);//控制最小时间
        }
        private IEnumerator OnLoadPracticeSkill()
        {
            float minWaitTime = 3f;
            float enterTime = Time.unscaledTime;
            yield return new WaitUntil(() => App.isInited);//等待初始化完成

            //yield return Load<GameScene>(false, GameMode.PracticeSkill, GameLandManager.File_Time);

            yield return new WaitUntil(() => Time.unscaledTime - enterTime > minWaitTime);//控制最小时间
        }

        private IEnumerator OnLoadWanted()
        {
            float minWaitTime = 3f;
            float enterTime = Time.unscaledTime;
            yield return new WaitUntil(() => App.isInited);//等待初始化完成

            //yield return Load<GameScene>(false, GameMode.Wanted, GameLandManager.
                //File_Time);

            yield return new WaitUntil(() => Time.unscaledTime - enterTime > minWaitTime);//控制最小时间
        }

        private IEnumerator OnLoadTower()
        {
            float minWaitTime = 3f;
            float enterTime = Time.unscaledTime;
            yield return new WaitUntil(() => App.isInited);//等待初始化完成

            //yield return Load<GameScene>(false, GameMode.Tower, GameLandManager.
            //    File_Time);

            yield return new WaitUntil(() => Time.unscaledTime - enterTime > minWaitTime);//控制最小时间
        }
        private IEnumerator OnLoadArena()
        {
            float minWaitTime = 3f;
            float enterTime = Time.unscaledTime;
            yield return new WaitUntil(() => App.isInited);//等待初始化完成

            //yield return Load<GameScene>(false, GameMode.Arena, GameLandManager.
            //    File_Time);

            yield return new WaitUntil(() => Time.unscaledTime - enterTime > minWaitTime);//控制最小时间
        }
    }
}