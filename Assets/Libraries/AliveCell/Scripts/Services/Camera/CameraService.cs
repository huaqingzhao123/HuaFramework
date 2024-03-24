/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/11/10 11:11:23
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using XMLib.FSM;

namespace AliveCell
{
    public partial class GlobalSetting
    {
        [SerializeField]
        private CameraService.Setting _camera = null;

        public static CameraService.Setting camera => Inst._camera;
    }

    /// <summary>
    /// CameraService
    /// </summary>
    public class CameraService : IServiceInitialize, IDisposable, IMonoLateUpdate
    {
        [Serializable]
        public class Setting
        {
            public CameraFollowSetting follow;
            public CameraShakeSetting shake;
            public CameraEffectSetting effect;

            #region Tool

            public List<CameraShakeInfo> shakeInfos => shake.shakeInfos;
            public List<CameraFollowInfo> followInfos => follow.followInfos;
            [NonSerialized] protected Dictionary<string, CameraShakeInfo> _cacheShake;
            [NonSerialized] protected Dictionary<string, CameraFollowInfo> _cacheFollow;
            [NonSerialized] protected bool _isInited = false;

            public CameraShakeInfo GetShake(string name)
            {
                Initialize();
                return _cacheShake[name];
            }

            public CameraFollowInfo GetFollow(string name)
            {
                Initialize();
                return _cacheFollow[name];
            }

            private void Initialize()
            {
                if (_isInited)
                {
                    return;
                }
                _isInited = true;

                _cacheShake = new Dictionary<string, CameraShakeInfo>();

                foreach (var item in shakeInfos)
                {
                    _cacheShake[item.name] = item;
                }

                _cacheFollow = new Dictionary<string, CameraFollowInfo>();

                foreach (var item in followInfos)
                {
                    _cacheFollow[item.name] = item;
                }
            }

            #endregion Tool
        }

        public Setting setting => GlobalSetting.camera;

        public CameraShake shake { get; private set; }
        public CameraFollow follow { get; private set; }

        public CameraEffect effect { get; private set; }

        [InjectObject] public ResourceService res { get; set; }
        [InjectObject] public DeviceService device { get; set; }

        public CameraRoot cameraRoot { get; private set; }
        public Camera camera => cameraRoot.camera;
        public Transform rootTransform => cameraRoot.root;
        public Transform cameraTransform => cameraRoot.cameraRoot;
        public AudioListener listener => cameraRoot.listener;

        public CameraFSM fsm { get; private set; }

        public List<ICameraOperation> operations = new List<ICameraOperation>();

        public SuperLogHandler LogHandler = SuperLogHandler.Create("CAM");

        public float timeScale { get; set; } = 1f;
        public bool isRunning { get; set; } = true;

        public T AddOperation<T>(T op) where T : ICameraOperation
        {
            operations.Add(op);
            return op;
        }

        public CameraService()
        {
            this.shake = AddOperation(new CameraShake(this));
            this.follow = AddOperation(new CameraFollow(this));
            this.effect = AddOperation(new CameraEffect(this));
        }

        public void Dispose()
        {
            if (cameraRoot != null)
            {
                App.DestroyGO(cameraRoot);
                cameraRoot = null;
            }
        }

        public IEnumerator OnServiceInitialize()
        {
            GameObject obj = res.CreateGO(10002001);
            obj.name = "[AC]Camera";
            GameObject.DontDestroyOnLoad(obj);
            cameraRoot = obj.GetComponent<CameraRoot>();

            //启动所有操作
            foreach (var op in operations)
            {//默认都启动
                op.SetActive(true);
            }

            //fsm初始化
            fsm = new CameraFSM();
            fsm.ChangeState<CameraStates.None>();

            yield break;
        }

        public void OnMonoLateUpdate()
        {
            if (!isRunning)
            {
                return;
            }

            float deltaTime = Time.deltaTime * timeScale;
            fsm.Update(this, deltaTime);
            foreach (var op in operations)
            {
                if (op.isActive)
                {
                    op.Update(deltaTime);
                }
            }
        }

        public void Shake(string name)
        {
            Shake(setting.GetShake(name));
        }

        public void Shake(CameraShakeInfo info)
        {
            shake.Shake(info);
        }
    }
}