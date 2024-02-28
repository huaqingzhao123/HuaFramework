using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Nireus
{
    public enum UiEffectLyaerMask
    {
        Scroll0 =  15,
        Scroll1 =  16,
        Scroll2    =  17,
        Scroll3 = 18,
    }
    public class UIEffectManager : SingletonBehaviour<UIEffectManager>
    {
        private Dictionary<string, EffectPool> nameFxPools = new Dictionary<string, EffectPool>();
        private Dictionary<GameObject, EffectPool> goFxPools = new Dictionary<GameObject, EffectPool>();

        private float timeScaleDuration;
        private bool isTimeScale;
        private float curTimeScale;
        private Camera uiCamera;
        private Camera worldCamera;

        public override void Initialize()
        {
            base.Initialize();
            uiCamera = GameObject.FindGameObjectWithTag("UICamera").GetComponent<Camera>();
            worldCamera = Camera.main;
        }

        void Awake()
        {
            curTimeScale = Time.timeScale;
        }

        void Destory()
        {
            ReleaseAllFxPool();
        }

        void Update()
        {
            UpdateTimeScale();
        }

        private EffectController CreateFx(string file_name, bool ignoreTimeScale = false, float beginTime = 0f, float life = 10f)
        {
            EffectPool fxPool;
            if (nameFxPools.TryGetValue(file_name, out fxPool) == false)
            {
                string full_name = PathConst.PATH_UI_EFFECT + file_name+".prefab";
                //GameObject obj = GameObject.Instantiate(Nireus.AssetManager.getInstance().loadSync(full_name)) as GameObject;
                GameObject obj = PrefabLoadManager.Instance.LoadSync(full_name);
                if (obj == null)
                {
                    return null;
                }

                fxPool = new EffectPool(this.transform, obj);

                nameFxPools.Add(file_name, fxPool);
            }

            return fxPool.Spawn(ignoreTimeScale, beginTime, life);
        }


        public EffectController Play(string file_name, Transform parent,bool attachedToParent, bool autoDestroy, float beginTime = 0f, float lifeTime = 10f)
        {
            EffectController fx = CreateFx(file_name, false, beginTime, lifeTime);
            if (fx == null)
            {
                GameDebug.LogError("can't found " + file_name);
                return null;
            }

            fx.autoDestroy = autoDestroy;

            if (attachedToParent)
            {
                SetParentAndResetLocalTrans(fx.Root,parent.transform);
            }
            else
            {
                fx.Root.position = parent.transform.position;
                fx.Root.rotation = parent.transform.rotation;
            }

            int ui_layer = 5;//unity 默认为UI Layer = 5 
            // Set layer
            if (fx.gameObject.layer != ui_layer)
            {
                SetLayerRecursively(fx.gameObject,ui_layer);
            }

            if (autoDestroy)
            {
                fx.autoDestroy = true;
                fx.loop = false;
            }
            fx.life = lifeTime;
            return fx;
        }

        public void Despawn(EffectController effect)
        {
            effect.DestroyFX();
        }


        public void FreePooledFx()
        {
            foreach (var kvp in nameFxPools)
                kvp.Value.FreePooledFx();

            foreach (var kvp in goFxPools)
                kvp.Value.FreePooledFx();
        }

        public void ReleaseAllFxPool()
        {
            foreach (var kvp in nameFxPools)
                kvp.Value.Release();
            nameFxPools.Clear();

            foreach (var kvp in goFxPools)
                kvp.Value.Release();
            goFxPools.Clear();
        }

        public void ScaleTime(float scale, float duration)
        {
            if (scale < 0 || duration < 0)
                return;

            isTimeScale = true;
            Time.timeScale = scale;
            curTimeScale = scale;
            timeScaleDuration = Time.realtimeSinceStartup + duration;
        }

        public void ResumeTimeScale()
        {
            timeScaleDuration = 0;
        }

        public float GetScaleTime()
        {
            return curTimeScale;
        }

        private void UpdateTimeScale()
        {
            if (isTimeScale)
            {
                if (timeScaleDuration < Time.realtimeSinceStartup)
                {
                    Time.timeScale = 1.0f;
                    curTimeScale = 1.0f;
                    isTimeScale = false;
                }
            }
        }

        
        void SetParentAndResetLocalTrans(Transform child, Transform parent)
        {
            child.SetParent(parent);
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
        }


        void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Component child in go.GetComponentInChildren<Transform>(true))
            {
                child.gameObject.layer = layer;
                SetLayerRecursively(child.gameObject, layer);
            }
        }


    }


}