/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/10/25 17:14:31
 */

using AliveCell.Commons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using XMLib;
using XMLib.AM;
using Debug = UnityEngine.Debug;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

using FPPhysics;
using Vector2 = FPPhysics.Vector2;
using Vector3 = FPPhysics.Vector3;
using Quaternion = FPPhysics.Quaternion;
using Matrix4x4 = FPPhysics.Matrix4x4;
using Mathf = FPPhysics.FPUtility;
using Single = FPPhysics.Fix64;
using FPPhysics.EntityStateManagement;

namespace AliveCell
{
    public enum GameState
    {
        Normal,
        Successed,
        Failed
    }

    [Flags]
    public enum GameMode
    {
        Normal = 0b0000_0001,
        Net = 0b0000_0010,
        Replay = 0b0000_0100,
        PVP = 0b0000_1000,

        All = ~0b0000_0000,
        None = 0b0000_0000,
    }

    public partial class GlobalSetting
    {
        [SerializeField]
        private GameWorld.Setting _gameWorld = null;

        public static GameWorld.Setting gameWorld => Inst._gameWorld;
    }

    /// <summary>
    /// GameWorld
    /// </summary>
    public abstract partial class GameWorld : IWorld, IMonoUpdate, IMonoDestroy, IMonoStart, IMonoLateUpdate, IDisposable
    {
        public UInputSystem uinput;
        public UGameSystem ugame;
        public UObjectSystem uobj;
        public UViewSystem uview;
        public UEventSystem uevt;
        public UAudioSystem uaudio;

        public USceneSystem uscene;
        public UPlayerSystem uplayer;
        public UEnemySystem uenemy;
        public UPhysicSystem uphysic;
        public UCameraSystem ucamera;
        public UPopTextSystem upopText;

        public SuperLogHandler LogHandler;

        public GameMode gameMode { get; protected set; }

        [Serializable]
        public partial class Setting
        {
        }

#if UNITY_EDITOR
        public UTestSystem test;
#endif

        public Setting setting => GlobalSetting.gameWorld;

        public static readonly Single logicFrameRate = 0.033333m;
        protected float logicTimer = logicFrameRate;

        public int frameIndex { get; protected set; } = -1;
        public int renderIndex { get; protected set; } = -1;

        public int logicFrameInRenderFrameUpdateCount { get; protected set; } = 0;
        public bool renderFrameNotUpdate => logicFrameInRenderFrameUpdateCount > 0;

        [InjectObject] public ResourceService res { get; set; }
        [InjectObject] public UIManager ui { get; set; }
        [InjectObject] public AudioService audio { get; set; }
        [InjectObject] public SceneService scene { get; set; }
        [InjectObject] public CameraService camera { get; set; }
        [InjectObject] public InputService input { get; set; }
        //[InjectObject] public RecordService record { get; set; }
        [InjectObject] public ArchiveService archive { get; set; }
        [InjectObject] public DeviceService device { get; set; }

        public bool isRunning { get; set; } = false;
        public GameState gameState { get; protected set; } = GameState.Normal;

        public float timeScale { get; set; } = 1;
        public float deltaTime => timeScale * Time.deltaTime;

        protected float syncLogicUpdateTimer { get; set; } = 0;

        public virtual string selfPlayerId { get; }

        public LevelController levelController { get; private set; }

        protected int randomCount = 0;
        protected Random random;

        public int RandInt(int min, int max)
        {
            randomCount++;
            return random.NextInt(min, max);
        }

        public static object OnCreateGameWorld(Container container, object[] userParams)
        {
            GameMode mode = (GameMode)userParams[0];
            Type concrete = null;

            switch (mode)
            {
                case GameMode.Normal:
                    break;

                case GameMode.Net:
                    break;

                case GameMode.Replay:
                    break;

                case GameMode.PVP:
                    break;

                default:
                    SuperLog.LogWarning($"不存在该模式:{mode}");
                    break;
            }

            return container.CreateInstance<GameWorld>(concrete, userParams);
        }

        public GameWorld(GameMode gameMode)
        {
            random = new Random(999);
            LogHandler = SuperLogHandler.Create(() => $"GW({this.gameMode}),{this.frameIndex}");
            this.gameMode = gameMode;
        }

        public virtual void Dispose()
        {
            LogHandler.Log($"GameWorld 释放, randomCount:{randomCount}");
        }

        public void LogicUpdate(Single deltaTime)
        {
            DrawUtility.D.PushDuration(deltaTime);
            LogicUpdateSystem(deltaTime);
            DrawUtility.D.PopDuration();
        }

        public void OnMonoDestroy()
        {
            App.Off(this);
            DestroySystem();
        }

        public virtual void OnMonoStart()
        {
            CreateSystem();
        }

        public virtual void OnMonoUpdate()
        {
            DrawUtility.D.PushDuration(0);
            UpdateSystem(deltaTime);
            DrawUtility.D.PopDuration();
        }

        public virtual void OnMonoLateUpdate()
        {
            DrawUtility.D.PushDuration(0);
            LateUpdateSystem(deltaTime);
            DrawUtility.D.PopDuration();
        }

        public virtual IEnumerator Initialize()
        {
            LogHandler.Log($"Initialize");

            levelController = GameObject.FindObjectOfType<LevelController>();

            yield break;
        }

        public virtual IEnumerator UnInitialize()
        {
            LogHandler.Log("UnInitialize");
            yield break;
        }

        public abstract GameState CheckCompleted();

        public GameState CheckPVECompleted()
        {
            if (gameState != GameState.Normal)
            {
                return gameState;
            }
            if (uobj.Count<EnemyObject>(t => !t.isDead) <= 0)
            {
                return GameState.Successed;
            }
            else if (uobj.Count<PlayerObject>(t => !t.isDead) <= 0)
            {
                return GameState.Failed;
            }

            return GameState.Normal;
        }

        public GameState CheckPVPCompleted()
        {
            if (gameState != GameState.Normal)
            {
                return gameState;
            }
            PlayerObject player = uplayer.GetSelfPlayer();
            if (player == null || player.isDead)
            {
                return GameState.Failed;
            }

            bool hasPartyAlive = uplayer.uids.Count <= 1;
            foreach (var uid in uplayer.uids)
            {
                if (player.ID == uid)
                {//排除自己
                    continue;
                }

                PlayerObject partyPlayer = uplayer.GetPlayer(uid);
                if (partyPlayer != null && !partyPlayer.isDead)
                {
                    hasPartyAlive = true;
                }
            }

            if (!hasPartyAlive)
            {
                return GameState.Successed;
            }

            return GameState.Normal;
        }

        protected virtual void OnLogicUpdateEnd(Single deltaTime)
        {
        }

        protected virtual void OnLogicUpdateBegin(Single deltaTime)
        {
        }

        #region game

        public virtual void CompletedGame(GameState state)
        {
            LogHandler.Log($"CompletedGame {state}");
            gameState = state;
            App.Trigger(EventTypes.Game_Complete);
        }

        #endregion game

        #region pre create

        protected PlayerObject PreCreatePlayer(int playerType, string playerID, int groupId, Vector3 position, Quaternion rotation)
        {
            var info = res.LoadPlayerInfo(playerType);
            return PreCreate<PlayerObject>(info.prefabID, (t) =>
            {
                t.playerID = playerID;
                t.groupID = groupId;
                t.configName = info.config;
                t.info = info;

                t.initData = new RigidBodyInitData()
                {
                    physic = setting.physic.player,
                    Position = position,
                    Orientation = rotation
                };

                t.attackMask = LayerMask.GetMask("Enemy", "Player");
                t.layer = LayerMask.NameToLayer("Player");
            });
        }

        protected EnemyObject PreCreateEnemy(int enemyType, Vector3 position, Quaternion rotation)
        {
            var info = res.LoadEnemyInfo(enemyType);
            return PreCreate<EnemyObject>(info.prefabID, (t) =>
            {
                t.info = info;
                t.groupID = 1000;
                t.configName = info.config;

                t.initData = new RigidBodyInitData()
                {
                    physic = setting.physic.player,
                    Position = position,
                    Orientation = rotation
                };

                t.attackMask = LayerMask.GetMask("Player");
                t.layer = LayerMask.NameToLayer("Enemy");
            });
        }

        #endregion pre create

        #region obj

        protected T PreCreate<T>(int prefabID, Action<T> initCallback = null) where T : TObject, new()
        {
            T result = uobj.PreCreate<T>((t) =>
            {
                t.prefabID = prefabID;
                initCallback?.Invoke(t);
            });

            return result;
        }

        public T Create<T>(int prefabID, Action<T> initCallback = null) where T : TObject, new()
        {
            return uobj.Create<T>((t) =>
            {
                t.prefabID = prefabID;
                initCallback?.Invoke(t);
            });
        }

        public T Create<T>() where T : UObject, new()
        {
            return uobj.Create<T>();
        }

        public void Destroy<T>(T obj) where T : UObject
        {
            this.uobj.Destroy(obj);
        }

        public void Destroy(int id)
        {
            this.uobj.Destroy(id);
        }

        public void DestroyAll()
        {
            uobj.DestroyAll();
        }

        /// <summary>
        /// 包含所有已有，新添加，新删除的对象
        /// </summary>
        public UObject FindObjWithID(int id)
        {
            return uobj.Get(id);
        }

        /// <summary>
        /// 不包含已删除的对象
        /// </summary>
        public T FindObjWithID<T>(int id) where T : UObject
        {
            return uobj.Get<T>(id);
        }

        #endregion obj

        #region system

        private readonly List<ISystem> onSys = new List<ISystem>();
        private readonly List<ICreate> onSysCreate = new List<ICreate>();
        private readonly List<IDestroy> onSysDestroy = new List<IDestroy>();
        private readonly List<ILogicUpdate> onSysLogicUpdate = new List<ILogicUpdate>();
        private readonly List<ILogicUpdateBegin> onSysLogicUpdateBegin = new List<ILogicUpdateBegin>();
        private readonly List<ILogicUpdateEnd> onSysLogicUpdateEnd = new List<ILogicUpdateEnd>();
        private readonly List<IUpdate> onSysUpdate = new List<IUpdate>();
        private readonly List<ISyncLogicUpdate> onSysSyncLogicUpdate = new List<ISyncLogicUpdate>();
        private readonly List<ILateUpdate> onSysLateUpdate = new List<ILateUpdate>();
        private readonly HashSet<ISystem> systemsHash = new HashSet<ISystem>();

        private GameWorld AddSystem<T>(T sys) where T : ISystem
        {
            if (!systemsHash.Add(sys))
            {
                throw new RuntimeException($"{nameof(ISystem)} 已经添加");
            }

            onSys.Add(sys);
            if (sys is ICreate onCreate) { onSysCreate.Add(onCreate); }
            if (sys is IUpdate onUpdate) { onSysUpdate.Add(onUpdate); }
            if (sys is ISyncLogicUpdate onSyncLogicUpdate) { onSysSyncLogicUpdate.Add(onSyncLogicUpdate); }
            if (sys is ILateUpdate onLateUpdate) { onSysLateUpdate.Add(onLateUpdate); }
            if (sys is IDestroy onDestroy) { onSysDestroy.Add(onDestroy); }
            if (sys is ILogicUpdate onLogicUpdate) { onSysLogicUpdate.Add(onLogicUpdate); }
            if (sys is ILogicUpdateBegin onLogicUpdateBegin) { onSysLogicUpdateBegin.Add(onLogicUpdateBegin); }
            if (sys is ILogicUpdateEnd onLogicUpdateEnd) { onSysLogicUpdateEnd.Add(onLogicUpdateEnd); }

            return this;
        }

        private void RemoveSystem<T>(T sys) where T : ISystem
        {
            if (!systemsHash.Contains(sys))
            {
                throw new RuntimeException($"{nameof(ISystem)} 已经移除");
            }

            systemsHash.Remove(sys);
            if (sys is ICreate onCreate) { onSysCreate.Remove(onCreate); }
            if (sys is IUpdate onUpdate) { onSysUpdate.Remove(onUpdate); }
            if (sys is ISyncLogicUpdate onSyncLogicUpdate) { onSysSyncLogicUpdate.Remove(onSyncLogicUpdate); }
            if (sys is ILateUpdate onLateUpdate) { onSysLateUpdate.Remove(onLateUpdate); }
            if (sys is IDestroy onDestroy) { onSysDestroy.Remove(onDestroy); }
            if (sys is ILogicUpdate onLogicUpdate) { onSysLogicUpdate.Remove(onLogicUpdate); }
            if (sys is ILogicUpdateBegin onLogicUpdateBegin) { onSysLogicUpdateBegin.Remove(onLogicUpdateBegin); }
            if (sys is ILogicUpdateEnd onLogicUpdateEnd) { onSysLogicUpdateEnd.Remove(onLogicUpdateEnd); }
        }

        private void AddAllSystem()
        {
            //添加系统-顺序不要轻易改变
            this
                .AddSystem(uinput = new UInputSystem(this, camera, input))//必须放在第一位，用于更新清除输入
                .AddSystem(ugame = new UGameSystem(this))
                .AddSystem(uobj = new UObjectSystem(this))
                .AddSystem(uevt = new UEventSystem(this))//执行先后顺序存在问题，在事件中删除/增加对象，如果后续需要检查删除了哪些，EventSystem后面的系统可以检查到，目前紧跟ObjectSystem，且事件只在ObjectSystem中使用
                .AddSystem(uscene = new USceneSystem(this))
                .AddSystem(uphysic = new UPhysicSystem(this))
                .AddSystem(uplayer = new UPlayerSystem(this))
                .AddSystem(uenemy = new UEnemySystem(this))
                .AddSystem(uaudio = new UAudioSystem(this))
                .AddSystem(upopText = new UPopTextSystem(this))
                .AddSystem(uview = new UViewSystem(this))
                .AddSystem(ucamera = new UCameraSystem(this, camera))
#if UNITY_EDITOR
                .AddSystem(test = new UTestSystem(this))
#endif
                ;
        }

        private void RemoveAllSystem()
        {
            onSysCreate.Clear();
            onSysUpdate.Clear();
            onSysSyncLogicUpdate.Clear();
            onSysLateUpdate.Clear();
            onSysDestroy.Clear();
            onSysLogicUpdate.Clear();
            onSysLogicUpdateBegin.Clear();
            onSysLogicUpdateEnd.Clear();

            uinput = null;
            ugame = null;
            uobj = null;
            uevt = null;
            uscene = null;
            uphysic = null;
            uplayer = null;
            uenemy = null;
            uaudio = null;
            uview = null;
            ucamera = null;
#if UNITY_EDITOR
            test = null;
#endif
        }

        protected void InitializeSystem(List<UObject> objs)
        {
            foreach (var sys in onSys)
            {
                sys.OnInitialize(objs);
            }
        }

        private void CreateSystem()
        {
            AddAllSystem();
            foreach (var sys in onSysCreate)
            {
                sys.OnCreate();
            }
        }

        private void DestroySystem()
        {
            List<IDestroy> onSystems = new List<IDestroy>(onSysDestroy);

            //反向删除
            onSystems.Reverse();
            foreach (var sys in onSystems)
            {
                sys.OnDestroy();
            }
            RemoveAllSystem();
        }

        private void LogicUpdateSystem(Single deltaTime)
        {
            if (!isRunning)
            {
                return;
            }

            frameIndex++;
            syncLogicUpdateTimer += deltaTime;

            OnLogicUpdateBegin(deltaTime);

            foreach (var sys in onSysLogicUpdateBegin)
            {
                sys.OnLogicUpdateBegin();
            }

            foreach (var sys in onSysLogicUpdate)
            {
                sys.OnLogicUpdate(deltaTime);
            }

            foreach (var sys in onSysLogicUpdateEnd)
            {
                sys.OnLogicUpdateEnd();
            }

            OnLogicUpdateEnd(deltaTime);

            logicFrameInRenderFrameUpdateCount++;

#if UNITY_EDITOR
            //if (logicFrameInRenderFrameUpdateCount > 1)
            //{
            //    LogHandler.Log($"Time.unscaledDeltaTime:{Time.unscaledDeltaTime}\nTime.deltaTime:{Time.deltaTime},Time.frameCount:{Time.frameCount} logicDeltaTime:{logicDeltaTime}  logicFrameIndex:{frameIndex} logicFrameInRenderFrameUpdateCount:{logicFrameInRenderFrameUpdateCount}");
            //}
#endif
        }

        private void UpdateSystem(float deltaTime)
        {
            if (!isRunning)
            {
                return;
            }
            renderIndex++;

            if (syncLogicUpdateTimer > Single.Epsilon)
            {
                Single syncDeltaTime = (Single)(syncLogicUpdateTimer > deltaTime ? deltaTime : syncLogicUpdateTimer);
                syncLogicUpdateTimer -= (float)syncDeltaTime;

                foreach (var sys in onSysSyncLogicUpdate)
                {
                    sys.OnSyncLogicUpdate(syncDeltaTime);
                }
            }

            foreach (var sys in onSysUpdate)
            {
                sys.OnUpdate(deltaTime);
            }

            logicFrameInRenderFrameUpdateCount = 0;//清空
        }

        private void LateUpdateSystem(float deltaTime)
        {
            if (!isRunning)
            {
                return;
            }

            foreach (var sys in onSysLateUpdate)
            {
                sys.OnLateUpdate(deltaTime);
            }
        }

        #endregion system
    }
}