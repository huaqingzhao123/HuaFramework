using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AliveCell;
using Nireus;
using UnityEngine;
using XMLib;
using Application = XMLib.Application;
using EventHandler = XMLib.EventHandler;
public enum LaunchStatus
{
    None = 0,
    Initing,
    Inited,
    TypeReged,
    TypeReging,
    ServiceCreating,
    ServiceCreated,
    ServiceIniting,
    ServiceInited,
    ServiceLastIniting,
    ServiceLastInited,
}
public enum LaunchMode
{
    None = 0,
    Normal = 1,
    HangUp = 2,
    Escort = 3,
    PracticeSkill = 4,
    Wanted = 5,
    Tower = 6,
    Arena = 7,
}
/// <summary>
/// App
/// </summary>
public static class App
{
    public static Application app { get; private set; }

    public static bool isInited => launchStatus == LaunchStatus.Inited;
    public static LaunchMode launchMode { get; private set; } = LaunchMode.Normal;
    public static LaunchStatus launchStatus { get; private set; } = LaunchStatus.None;
    public static UnityApplication unityApp { get; private set; }
    public static Action onInitialized { get; set; }
    public static Action onDisposed { get; set; }

    public static Vector3 sceneOffset = new Vector3(100, 0, 50);
    public static Vector3 birthOffset = new Vector3(0, 0, -20);

    public readonly static Type[] serviceTypes = new Type[] {
            typeof(ResourceService),
            typeof(DeviceService),
            //typeof(AudioService),
            typeof(InputService),
            typeof(CameraService),
            //typeof(UIManager),
            //typeof(AliveCell.Network.NetworkService),
            //typeof(RecordService),
            typeof(SceneService),
        };

    public static void Initialize(LaunchMode launchMode = LaunchMode.Normal)
    {
        if (null != app)
        {
            GameDebug.LogError("Application 已经创建");
            OnDispose();
        }

        App.launchMode = launchMode;//启动模式
        SuperLog.tag = "AS";

        //GlobalSetting.Inst = AssetManager.Instance.loadSync<GlobalSetting>($"{PathConst.BUNDLE_RES}FightConfig/Global Setting.asset");
        DG.Tweening.DOTween.Init();

        GameObject obj = new GameObject("[AC]App", typeof(UnityApplication));
        //obj.transform.SetParent(GameLandManager.Instance.Dynamic_Create_Root);
        unityApp = obj.GetComponent<UnityApplication>();
        unityApp.onDestroyed += OnDispose;
        unityApp.StartCoroutine(_OnInitialize());
    }

    private static IEnumerator _OnInitialize()
    {
        launchStatus = LaunchStatus.Initing;
        app = new Application();
        yield return InitializeService(app);
        unityApp.app = app;
        launchStatus = LaunchStatus.Inited;
        onInitialized?.Invoke();
        onInitialized = null;
    }

    private static void OnDispose()
    {
        app.Flush();
        launchStatus = LaunchStatus.None;
        onDisposed?.Invoke();
        onDisposed = null;
        app = null;
        _camera = null;
        _input = null;
        _res = null;
        _obj = null;
        _scene = null;
        //_record = null;
        _device = null;
        game = null;
        unityApp = null;
    }

    private static IEnumerator InitializeService(Application target)
    {
        //初始化基础服务
        using (TimeWatcher watcher = new TimeWatcher("服务初始化"))
        {
            //注册普通服务
            launchStatus = LaunchStatus.TypeReging;
            watcher.Start();

            target.Singleton<GameWorld>(GameWorld.OnCreateGameWorld)
                .OnAfterResolving((_, o) =>
                {
                    game = o as GameWorld;
                })
                .OnRelease((_, o) =>
                {
                    game = null;
                });



            watcher.End($"注册普通服务");
            launchStatus = LaunchStatus.TypeReged;
            //=======================================

            //注册基础服务
            launchStatus = LaunchStatus.ServiceCreating;
            List<IServiceInitialize> initServices = new List<IServiceInitialize>(serviceTypes.Length);
            List<IServiceLateInitialize> lateInitServices = new List<IServiceLateInitialize>(serviceTypes.Length);
            foreach (var serviceType in serviceTypes)
            {
                watcher.Start();
                target.Singleton(serviceType);
                object obj = target.Make(serviceType);
                if (obj is IServiceInitialize init)
                {//需要初始化
                    initServices.Add(init);
                }
                if (obj is IServiceLateInitialize lateInit)
                {//需要后初始化
                    lateInitServices.Add(lateInit);
                }
                watcher.End($"注册并创建 [{serviceType}] 服务");
            }
            launchStatus = LaunchStatus.ServiceCreated;
            //=========================================
            //初始化服务
            launchStatus = LaunchStatus.ServiceIniting;
            foreach (var service in initServices)
            {
                watcher.Start();
                yield return service.OnServiceInitialize();
                watcher.End($"初始化 [{service.GetType()}] 服务");
            }

            launchStatus = LaunchStatus.ServiceInited;
            //=========================================
            //后初始化服务
            launchStatus = LaunchStatus.ServiceLastIniting;
            foreach (var service in lateInitServices)
            {
                watcher.Start();
                yield return service.OnServiceLateInitialize();
                watcher.End($"后初始化 [{service.GetType()}] 服务");
            }
            launchStatus = LaunchStatus.ServiceLastInited;
            //SceneService.SceneInfo info = App.scene.GetInfo();

            //App.Trigger(EventTypes.UI_StartGame);
            //========================================
        }

        yield break;
    }

    #region static

    public static EventDispatcher evt => _event ?? (_event = app.Make<EventDispatcher>());
    private static EventDispatcher _event;
    public static InputService input => _input ?? (_input = app.Make<InputService>());
    private static InputService _input;
    public static ResourceService res => _res ?? (_res = app.Make<ResourceService>());
    private static ResourceService _res;
    public static ObjectDriver obj => _obj ?? (_obj = app.Make<ObjectDriver>());
    private static ObjectDriver _obj;
    public static SceneService scene => _scene ?? (_scene = app.Make<SceneService>());
    private static SceneService _scene;
    // public static UIManager ui => _ui ?? (_ui = app.Make<UIManager>());
    // private static UIManager _ui;
    // public static AudioService audio => _audio ?? (_audio = app.Make<AudioService>());
    // private static AudioService _audio;
    //public static RecordService record => _record ?? (_record = app.Make<RecordService>());
    //private static RecordService _record;
    public static DeviceService device => _device ?? (_device = app.Make<DeviceService>());
    private static DeviceService _device;
    public static CameraService camera => _camera ?? (_camera = app.Make<CameraService>());
    private static CameraService _camera;
    // public static AliveCell.Network.NetworkService net => _net ?? (_net = app.Make<AliveCell.Network.NetworkService>());
    // private static AliveCell.Network.NetworkService _net;

    public static GameWorld game { get; private set; } = null;


    #endregion static

    #region Common

    public static bool isMainThread => app.isMainThread;

    public static void Run(Action callback)
    {
        unityApp.Run(callback);
    }

    public static Coroutine StartCoroutine(IEnumerator callback)
    {
        return unityApp.StartCoroutine(callback);
    }

    public static void StopCoroutine(Coroutine target)
    {
        unityApp.StopCoroutine(target);
    }

    #endregion Common

    #region Dispatcher

    public static EventHandler On(EventTypes eventType, object target, MethodInfo method, object group = null, bool matchingParams = false)
    {
        return evt.On((int)eventType, target, method, group, matchingParams);
    }

    public static void Trigger(EventTypes eventType, params object[] args)
    {
        //GameDebug.Log($"Trigger {eventType}");
        evt.Trigger((int)eventType, args);
    }

    public static void Off(object target)
    {
        evt.Off(target);
    }

    #region Extensions

    public static EventHandler On(EventTypes eventType, Action callback, object group = null, bool matchingParams = false)
    {
        return evt.On((int)eventType, callback.Target, callback.Method, group, matchingParams);
    }

    public static EventHandler On<T>(EventTypes eventType, Action<T> callback, object group = null, bool matchingParams = false)
    {
        return evt.On((int)eventType, callback.Target, callback.Method, group, matchingParams);
    }

    public static EventHandler On<T1, T2>(EventTypes eventType, Action<T1, T2> callback, object group = null, bool matchingParams = false)
    {
        return evt.On((int)eventType, callback.Target, callback.Method, group, matchingParams);
    }

    public static EventHandler On<T1, T2, T3>(EventTypes eventType, Action<T1, T2, T3> callback, object group = null, bool matchingParams = false)
    {
        return evt.On((int)eventType, callback.Target, callback.Method, group, matchingParams);
    }

    public static EventHandler On<T1, T2, T3, T4>(EventTypes eventType, Action<T1, T2, T3, T4> callback, object group = null, bool matchingParams = false)
    {
        return evt.On((int)eventType, callback.Target, callback.Method, group, matchingParams);
    }

    #endregion Extensions

    #endregion Dispatcher

    #region Container

    public static Container Alias(string alias, string service)
    {
        return app.Alias(alias, service);
    }

    public static BindData Bind(string service, Func<Container, object[], object> concrete, bool isStatic)
    {
        return app.Bind(service, concrete, isStatic);
    }

    public static BindData Bind(string service, Type concrete, bool isStatic)
    {
        return app.Bind(service, concrete, isStatic);
    }

    public static object Call(object target, MethodInfo methodInfo, params object[] userParams)
    {
        return app.Call(target, methodInfo, userParams);
    }

    public static bool CanMake(string service)
    {
        return app.CanMake(service);
    }

    public static void Flush()
    {
        app.Flush();
    }

    public static BindData GetBind(string service)
    {
        return app.GetBind(service);
    }

    public static bool HasBind(string service)
    {
        return app.HasBind(service);
    }

    public static bool HasInstance(string service)
    {
        return app.HasInstance(service);
    }

    public static object Instance(string service, object instance)
    {
        return app.Instance(service, instance);
    }

    public static bool IsAlias(string name)
    {
        return app.IsAlias(name);
    }

    public static bool IsResolved(string service)
    {
        return app.IsResolved(service);
    }

    public static bool IsStatic(string service)
    {
        return app.IsStatic(service);
    }

    public static object Make(string service, params object[] userParams)
    {
        return app.Make(service, userParams);
    }

    public static Container OnAfterResolving(Action<BindData, object> callback)
    {
        return app.OnAfterResolving(callback);
    }

    public static Container OnRebound(string service, Action<object> callback)
    {
        return app.OnRebound(service, callback);
    }

    public static Container OnRelease(Action<BindData, object> callback)
    {
        return app.OnRelease(callback);
    }

    public static Container OnResolving(Action<BindData, object> callback)
    {
        return app.OnResolving(callback);
    }

    public static bool Release(object mixed)
    {
        return app.Release(mixed);
    }

    public static string Type2Service(Type type)
    {
        return app.Type2Service(type);
    }

    public static void Unbind(string service)
    {
        app.Unbind(service);
    }

    #region Extensions

    public static Container Alias<TAlias, TService>()
    {
        return app.Alias<TAlias, TService>();
    }

    public static Container RemoveAlias<TAlias>()
    {
        return app.RemoveAlias<TAlias>();
    }

    public static Container ResetAlias<TAlias, TService>()
    {
        return app.ResetAlias<TAlias, TService>();
    }

    public static BindData Bind<T>()
    {
        return app.Bind<T>();
    }

    public static BindData Bind<TService, TConcrete>()
    {
        return app.Bind<TService, TConcrete>();
    }

    public static BindData Bind<TService>(Func<Container, object[], object> concrete)
    {
        return app.Bind<TService>(concrete);
    }

    public static BindData Bind<TService>(Func<object[], object> concrete)
    {
        return app.Bind<TService>(concrete);
    }

    public static BindData Bind(string service, Func<Container, object[], object> concrete)
    {
        return app.Bind(service, concrete);
    }

    public static void Call<T>(Action<T> method, params object[] userParams)
    {
        app.Call<T>(method, userParams);
    }

    public static void Call<T1, T2>(Action<T1, T2> method, params object[] userParams)
    {
        app.Call<T1, T2>(method, userParams);
    }

    public static void Call<T1, T2, T3>(Action<T1, T2, T3> method, params object[] userParams)
    {
        app.Call<T1, T2, T3>(method, userParams);
    }

    public static void Call<T1, T2, T3, T4>(Action<T1, T2, T3, T4> method, params object[] userParams)
    {
        app.Call<T1, T2, T3, T4>(method, userParams);
    }

    public static object Call(object target, string method, params object[] userParams)
    {
        return app.Call(target, method, userParams);
    }

    public static TService Make<TService>(params object[] userParams)
    {
        return app.Make<TService>(userParams);
    }

    public static bool CanMake<T>()
    {
        return app.CanMake<T>();
    }

    public static Func<TService> Factory<TService>(params object[] userParams)
    {
        return app.Factory<TService>(userParams);
    }

    public static Func<object> Factory(string service, params object[] userParams)
    {
        return app.Factory(service, userParams);
    }

    public static BindData GetBind<T>()
    {
        return app.GetBind<T>();
    }

    public static bool HasBind<T>()
    {
        return app.HasBind<T>();
    }

    public static bool HasInstance<T>()
    {
        return app.HasInstance<T>();
    }

    public static object Instance<TService>(object instance)
    {
        return app.Instance<TService>(instance);
    }

    public static bool IsAlias<T>()
    {
        return app.IsAlias<T>();
    }

    public static bool IsResolved<T>()
    {
        return app.IsResolved<T>();
    }

    public static bool IsStatic<T>()
    {
        return app.IsStatic<T>();
    }

    public static Container OnAfterResolving(Action<object> callback)
    {
        return app.OnAfterResolving(callback);
    }

    public static Container OnAfterResolving<T>(Action<T> callback)
    {
        return app.OnAfterResolving<T>(callback);
    }

    public static Container OnAfterResolving<T>(Action<BindData, T> callback)
    {
        return app.OnAfterResolving<T>(callback);
    }

    public static Container OnRelease(Action<object> callback)
    {
        return app.OnRelease(callback);
    }

    public static Container OnRelease<T>(Action<T> callback)
    {
        return app.OnRelease<T>(callback);
    }

    public static Container OnRelease<T>(Action<BindData, T> callback)
    {
        return app.OnRelease<T>(callback);
    }

    public static Container OnResolving(Action<object> callback)
    {
        return app.OnResolving(callback);
    }

    public static Container OnResolving<T>(Action<T> callback)
    {
        return app.OnResolving<T>(callback);
    }

    public static Container OnResolving<T>(Action<BindData, T> callback)
    {
        return app.OnResolving<T>(callback);
    }

    public static bool Release<TService>()
    {
        return app.Release<TService>();
    }

    public static BindData Singleton(string service, Func<Container, object[], object> concrete)
    {
        return app.Singleton(service, concrete);
    }

    public static BindData Singleton<TService, TConcrete>()
    {
        return app.Singleton<TService, TConcrete>();
    }

    public static BindData Singleton<TService>()
    {
        return app.Singleton<TService>();
    }

    public static BindData Singleton<TService>(Func<Container, object[], object> concrete)
    {
        return app.Singleton<TService>(concrete);
    }

    public static BindData Singleton<TService>(Func<object[], object> concrete)
    {
        return app.Singleton<TService>(concrete);
    }

    public static BindData Singleton<TService>(Func<object> concrete)
    {
        return app.Singleton<TService>(concrete);
    }

    public static string Type2Service<TService>()
    {
        return app.Type2Service<TService>();
    }

    public static void Unbind<TService>()
    {
        app.Unbind<TService>();
    }

    public static void Watch<TService>(Action method)
    {
        app.Watch<TService>(method);
    }

    public static void Watch<TService>(Action<TService> method)
    {
        app.Watch<TService>(method);
    }

    #endregion Extensions

    #endregion Container

    #region Resource

    public static GameObject FindPrefab(int prefabID)
    {
        return res.FindPrefab(prefabID);
    }

    public static GameObject CreateGO(int prefabID)
    {
        return res.CreateGO(prefabID);
    }

    public static void DestroyGO(GameObject obj)
    {
        res.DestroyGO(obj);
    }

    public static void DestroyGO<T>(T item) where T : IResourceItem
    {
        res.DestroyGO(item);
    }

    #endregion Resource

    #region Mono

    public static void AttachMono(object target)
    {
        obj.Attach(target);
    }

    public static bool ContainsMono(object target)
    {
        return obj.Contains(target);
    }

    public static bool DetachMono(object target)
    {
        return obj.Detach(target);
    }

    #endregion Mono
}
