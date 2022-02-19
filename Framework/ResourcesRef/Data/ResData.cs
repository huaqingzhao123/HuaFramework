using HuaFramework.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.ResourcesRef
{

    public enum AssetState
    {
        Waitting,
        Loading,
        Loaded
    }
    /// <summary>
    /// 资源数据类，自带引用计数
    /// </summary>
    public abstract class ResData : ResourceRefBase
    {
        public UnityEngine.Object Asset
        {
            get; protected set;
        }
        protected AssetState _assetState;
        public AssetState AssetState
        {

            get
            {
                return _assetState;
            }
            protected set
            {
                _assetState = value;
                if (_assetState == AssetState.Loaded)
                {
                    //调用加载完成事件
                    OnLoadedCallback?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// 加载完成的回调事件
        /// </summary>
        protected event Action<ResData> OnLoadedCallback;

        public void RegisterOnLoadedEvent(Action<ResData> action)
        {
            OnLoadedCallback += action;
        }
        public void UnRegisterOnLoadedEvent(Action<ResData> action)
        {
            OnLoadedCallback -= action;
        }
        public abstract bool LoadSync();

        public abstract void LoadAsync();

        public string AssetPath
        {
            get; protected set;
        }
        public string Name
        {
            get;
            protected set;
        }
        protected abstract void OnReleaseResources();
        protected override void OnZeroRef()
        {
            base.OnZeroRef();
            OnReleaseResources();
        }
    }

}
