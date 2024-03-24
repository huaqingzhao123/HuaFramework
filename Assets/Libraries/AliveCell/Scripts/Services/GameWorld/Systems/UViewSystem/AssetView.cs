/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/12/30 23:44:02
 */

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

using Single = FPPhysics.Fix64;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace AliveCell
{
    /// <summary>
    /// AssetView
    /// </summary>
    public abstract class AssetView : MonoBehaviour, IAssetView
    {
        [SerializeField]
        protected int _prefabID = 0;

        [SerializeField]
        protected int _preloadCnt = 0;

        [SerializeField]
        protected bool _useActive = true;

        [SerializeField]
        protected bool _forceDestroy = false;

        public bool useActive => _useActive;
        public int prefabID => _prefabID;
        public int preloadCnt => _preloadCnt;

        private float syncLogicUpdateTimer = 0f;

        public virtual bool canSyncLogic { get; } = true;
        public bool forceDestroy => _forceDestroy;

        #region pool

        public int poolTag => _prefabID;

        public bool inPool { get; set; }

        public int objID { get; set; }
        public UViewSystem system { get; set; }

        public GameWorld world => system.world;

        protected List<SubAssetView> subViews = new List<SubAssetView>();

        public T GetObj<T>() where T : class
        {
            return system.world.uobj.Get<T>(objID);
        }

        public virtual void OnPopPool()
        {
            if (subViews.Count > 0)
            {
                foreach (var item in subViews)
                {
                    item.OnPopPool();
                }
            }
        }

        public virtual void OnPushPool()
        {
            if (subViews.Count > 0)
            {
                foreach (var item in subViews)
                {
                    item.OnPushPool();
                }
            }

            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            objID = UObjectSystem.noneID;
            system = null;
            syncLogicUpdateTimer = 0f;
        }

        #endregion pool

        protected virtual void Awake()
        {
            GetComponentsInChildren<SubAssetView>(subViews);
            foreach (var item in subViews)
            {
                item.parentView = this;
            }
        }

        public virtual void LogicUpdateView(Single deltaTime)
        {
            syncLogicUpdateTimer += canSyncLogic ? deltaTime : 0f;

            if (subViews.Count > 0)
            {
                foreach (var item in subViews)
                {
                    item.LogicUpdateView(deltaTime);
                }
            }
        }

        public virtual void UpdateView(float deltaTime)
        {
            if (syncLogicUpdateTimer > Single.Epsilon)
            {
                Single syncDeltaTime = (Single)(syncLogicUpdateTimer > deltaTime ? deltaTime : syncLogicUpdateTimer);
                syncLogicUpdateTimer -= syncDeltaTime;
                SyncLogicUpdate(syncDeltaTime);
            }

            if (subViews.Count > 0)
            {
                foreach (var item in subViews)
                {
                    item.UpdateView(deltaTime);
                }
            }
        }

        protected virtual void SyncLogicUpdate(Single deltaTime)
        {
            if (subViews.Count > 0)
            {
                foreach (var item in subViews)
                {
                    item.SyncLogicUpdate(deltaTime);
                }
            }
        }

        public virtual void OnViewBind()
        {
            if (subViews.Count > 0)
            {
                foreach (var item in subViews)
                {
                    item.OnViewBind();
                }
            }
        }

        public virtual void OnViewUnbind()
        {
            if (subViews.Count > 0)
            {
                foreach (var item in subViews)
                {
                    item.OnViewUnbind();
                }
            }
        }
    }
}