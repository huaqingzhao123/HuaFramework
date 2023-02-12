using UnityEngine;
using System.Collections.Generic;

namespace Nireus
{ 
    public class EffectController : MonoBehaviour
    {
        [System.Serializable]
        public abstract class FXData
        {
            public float beginTime;
            public float endTime;
            protected bool played = false;
            protected bool ignoreTimeScale = false;
            public abstract bool IsDead { get; }
            public abstract void Play(bool isUnscaledDeltaTime);
            public abstract void Stop();
            public abstract void Reset(bool clearFX);
            public virtual void FadeOut(float remainTime) { }

            public virtual void Update(float playingTime)
            {
                if (playingTime >= beginTime)
                {
                    if (playingTime <= endTime)
                    {
                        if (played == false)
                            Play(ignoreTimeScale);
                    }
                    else if (endTime >= 0)
                    {
                        if (played)
                            Stop();
                    }
                }
            }

        }


        [System.Serializable]
        public class ParticleSystemData : FXData
        {
            public ParticleSystem particleSystem;

            public ParticleSystemData Create(ParticleSystem particleSystem)
            {
                ParticleSystem.MainModule main = particleSystem.main;
                main.playOnAwake = false;

                this.particleSystem = particleSystem;
                this.beginTime = 0;
                this.endTime = -1;
                return this;
            }

            public override bool IsDead
            {
                get { return particleSystem.particleCount == 0; }
            }

            public override void Play(bool ignoreTimeScale)
            {
                played = true;
                this.ignoreTimeScale = ignoreTimeScale;
                ParticleSystem.MainModule main = particleSystem.main;
                main.playOnAwake = true;
                particleSystem.Play();
            }

            public override void Stop()
            {
                ParticleSystem.MainModule main = particleSystem.main;
                main.playOnAwake = false;
                particleSystem.Stop();
            }

            public override void Reset(bool clearFX)
            {
                played = false;
                particleSystem.Stop();
                ParticleSystem.MainModule main = particleSystem.main;
                main.playOnAwake = false;
            }

            public override void FadeOut(float remainTime)
            {
                if (particleSystem.GetComponent<ParticleSystem>().particleCount == 0)
                    return;

                ParticleSystem.Particle[] particleDatas = new ParticleSystem.Particle[particleSystem.GetComponent<ParticleSystem>().particleCount];
                particleSystem.GetComponent<ParticleSystem>().GetParticles(particleDatas);
                ParticleSystem.Particle[] newParticleDatas = new ParticleSystem.Particle[particleDatas.Length];

                for (int j = 0; j < particleDatas.Length; j++)
                {
                    newParticleDatas[j] = new ParticleSystem.Particle();
                    newParticleDatas[j] = particleDatas[j];

                    if (newParticleDatas[j].remainingLifetime > remainTime)
                        newParticleDatas[j].remainingLifetime = remainTime;
                }

                particleSystem.GetComponent<ParticleSystem>().SetParticles(newParticleDatas, newParticleDatas.Length);
            }

            public override void Update(float playingTime)
            {
                if (ignoreTimeScale)
                {
                    particleSystem.Simulate(Time.unscaledDeltaTime, true, false);
                }
                base.Update(playingTime);
            }
        }

        [System.Serializable]
        public class AnimationData : FXData
        {
            public UnityEngine.Animation animation;
            
            public AnimationData Create(UnityEngine.Animation animation)
            {
                this.animation = animation;
                this.beginTime = 0;
                this.endTime = -1;
                return this;
            }

            public override bool IsDead
            {
                get { return animation.isPlaying == false; }
            }

            public override void Play(bool ignoreTimeScale)
            {
                this.ignoreTimeScale = ignoreTimeScale;
                played = true;
                animation.Play();
            }

            public override void Stop()
            {
                animation.Stop();
            }

            public override void Reset(bool clearFX)
            {
                played = false;
                animation.playAutomatically = false;
            }
        }

        [System.Serializable]
        public class TrailRendererData : FXData
        {
            public TrailRenderer renderer;

            public TrailRendererData Create(TrailRenderer renderer)
            {
                this.renderer = renderer;
                this.beginTime = 0;
                this.endTime = -1;
                return this;
            }

            public override bool IsDead
            {
                get { return renderer.enabled; }
            }

            public override void Play(bool isUnscaledDeltaTime)
            {
                this.ignoreTimeScale = isUnscaledDeltaTime;
                played = true;
                renderer.enabled = true;
            }

            public override void Stop()
            {
                renderer.enabled = false;
            }

            public override void Reset(bool clearFX)
            {
                played = false;
                renderer.enabled = false;
            }
        }

        
        public ParticleSystemData[] particleSystemDataArray;
        public AnimationData[] animationDataArray;
        public TrailRendererData[] trailRendererDataArray;
        public float life { get; set; }
        public bool loop = true;
        public bool autoDestroy = true;
        public bool ignoreTimeScale = false;
        public Transform followTarget;


        private float playingTime;
        private float lastFreeToPoolTime = 0;

        enum _STAGE
        {
            STOPPED,
            PLAYING,
            STOPPING,
        };

        private _STAGE playingStage = _STAGE.STOPPED;

        // End FX delegate	
        public delegate void OnFXFinishDelegate(object userData);

        private struct DelegateData
        {
            public OnFXFinishDelegate del;
            public object userData;

            public DelegateData(OnFXFinishDelegate del, object userData)
            {
                this.del = del;
                this.userData = userData;
            }
        }
        private List<DelegateData> onFXFinishDels;

        private Transform root;
        public Transform Root
        {
            get { return root != null ? root : this.transform; }
        }

        private EffectPool fxPool;

        public EffectPool FXPool
        {
            get { return fxPool; }
            set { fxPool = value; }
        }

        
        public void CreateRoot()
        {
            if (root == null)
            {
                root = new GameObject(this.gameObject.name+"parent").transform;
                // this.transform.SetParentAndResetLocalTrans(root);
                root.localPosition = Vector3.zero;
                root.localRotation = Quaternion.identity;
                root.localScale = Vector3.one;
                this.transform.SetParent(root,true);
                //this.transform.localPosition = Vector3.zero;
                //this.transform.localRotation = Quaternion.identity;
                //this.transform.localScale = Vector3.one;
            }
        }

        [ContextMenu("Play FX")]
        public void PlayFX(float beginTimeGlobal,float endTimeGlobal)
        {
            if (playingStage == _STAGE.PLAYING)
                return;

            playingStage = _STAGE.PLAYING;
            playingTime = 0;

            // Animations
            foreach (var data in particleSystemDataArray)
            {
                data.Reset(true);
                data.beginTime = beginTimeGlobal;
                data.endTime = endTimeGlobal;
                if (data.beginTime == 0)
                    data.Play(ignoreTimeScale);
            }

            // Animations
            foreach (var data in animationDataArray)
            {
                data.Reset(true);
                data.beginTime = beginTimeGlobal;
                data.endTime = endTimeGlobal;
                if (data.beginTime == 0)
                    data.Play(ignoreTimeScale);
            }

            foreach (var data in trailRendererDataArray)
            {
                data.Reset(true);
                data.beginTime = beginTimeGlobal;
                data.endTime = endTimeGlobal;
                if (data.beginTime == 0)
                    data.Play(ignoreTimeScale);
            }

        }

        [ContextMenu("Stop FX")]
        public void StopFX()
        {
            if (playingStage != _STAGE.PLAYING)
                return;

            playingStage = _STAGE.STOPPING;

            StopFXData();

        }


        //如果被挂在其他对象下，这个父对象被销毁，那么做一下回收
        private void OnDestroy()
        {
            if (FXPool != null)
                FXPool.OnDestroy(this);
        }


        public void DestroyFX()
        {
            if (FXPool != null)
                FXPool.Despawn(this);
            else
                Object.Destroy(this.Root.gameObject);
        }

        private void StopFXData()
        {
            foreach (var data in particleSystemDataArray)
                data.Stop();

            foreach (var data in animationDataArray)
                data.Stop();

            foreach (var data in trailRendererDataArray)
                data.Stop();
        }

        private void ResetFX(bool clearFX)
        {
            foreach (var data in particleSystemDataArray)
                data.Reset(clearFX);

            foreach (var data in animationDataArray)
                data.Reset(clearFX);

            foreach (var data in trailRendererDataArray)
                data.Reset(clearFX);
        }

        private void UpdateFX()
        {
            foreach (var data in particleSystemDataArray)
                data.Update(playingTime);

            foreach (var data in animationDataArray)
                data.Update(playingTime);

            foreach (var data in trailRendererDataArray)
                data.Update(playingTime);
        }



        [ContextMenu("Reset Array")]
        public void ResetArray()
        {
            ResetParticleSystemArray();
            ResetAnimationArray();
            ResetTrailRendererArray();
        }
        public void ResetParticleSystemArray()
        {
            ParticleSystem[] pfxSystems = gameObject.GetComponentsInChildren<ParticleSystem>();
            particleSystemDataArray = new ParticleSystemData[pfxSystems.Length];

            for (int i = 0; i < pfxSystems.Length; i++)
            {
                particleSystemDataArray[i] = new ParticleSystemData().Create(pfxSystems[i]);
            }
        }

        public void ResetAnimationArray()
        {
            var animations = gameObject.GetComponentsInChildren<UnityEngine.Animation>();
            animationDataArray = new AnimationData[animations.Length];

            for (int i = 0; i < animations.Length; ++i)
                animationDataArray[i] = new AnimationData().Create(animations[i]);
        }
        
        public void ResetTrailRendererArray()
        {
            TrailRenderer[] trailRenderer = gameObject.GetComponentsInChildren<TrailRenderer>();
            trailRendererDataArray = new TrailRendererData[trailRenderer.Length];

            for (int i = 0; i < trailRenderer.Length; ++i)
                trailRendererDataArray[i] = new TrailRendererData().Create(trailRenderer[i]);
        }

        [ContextMenu("Disable AutoPlay")]
        public void DisableAutoPlay()
        {
            ResetFX(true);
        }

        public void AddFinishCallback(OnFXFinishDelegate finishFun, object userData)
        {
            if (onFXFinishDels == null)
                onFXFinishDels = new List<DelegateData>();

            onFXFinishDels.Add(new DelegateData(finishFun, userData));
        }

        public void ClearFinishCallback()
        {
            if (onFXFinishDels != null)
                onFXFinishDels.Clear();
        }

        // Use this for initialization
        public void Init()
        {
            if ((particleSystemDataArray == null || particleSystemDataArray.Length == 0) &&
                (animationDataArray == null || animationDataArray.Length == 0) &&
                (trailRendererDataArray == null || trailRendererDataArray.Length == 0))
            {

                if ((particleSystemDataArray == null || particleSystemDataArray.Length == 0))
                    ResetParticleSystemArray();

                if ((animationDataArray == null || animationDataArray.Length == 0))
                    ResetAnimationArray();

                if ((trailRendererDataArray == null || trailRendererDataArray.Length == 0))
                    ResetTrailRendererArray();
            }
        }

        public void SetFreeToLastPoolTime(float time)
        {
            this.lastFreeToPoolTime = time;
        }

        public float GetNextPoolTime()
        {
            return lastFreeToPoolTime + GetDelayPoolTime();
        }

        private float GetDelayPoolTime()
        {
            float time = 0;
            foreach (var data in trailRendererDataArray)
                time = Mathf.Max(time, data.renderer.time);

            return time;
        }

        // Update is called once per frame
        private void LateUpdate()
        {
            if (playingStage == _STAGE.STOPPED)
                return;

            if (playingStage == _STAGE.PLAYING)
            {
                playingTime += Time.deltaTime;

                UpdateFX();

                if (playingTime > life)
                {
                    if (loop)
                    {
                        if (life > 0)
                        {
                            StopFXData();
                            ResetFX(false);
                            playingTime -= life;
                        }
                    }
                    else
                    {
                        StopFX();
                    }
                }
            }
            else if (playingStage == _STAGE.STOPPING)
            {
                // Life is end. 
                //if (IsDead())
                {
                    playingStage = _STAGE.STOPPED;
                }

                if (playingStage == _STAGE.STOPPED)
                {
                    playingTime = 0;

                    if (onFXFinishDels != null)
                        foreach (var delData in onFXFinishDels)
                            if (delData.del != null)
                                delData.del(delData.userData);

                    if (autoDestroy)
                        DestroyFX();
                }
            }
        }

        private bool IsDead()
        {

            foreach (var data in particleSystemDataArray)
                if (data.IsDead == false)
                    return false;

            foreach (var data in animationDataArray)
                if (data.IsDead == false)
                    return false;
            
            foreach (var data in trailRendererDataArray)
                if (data.IsDead == false)
                    return false;

            return true;
        }

        private void FadeOut(float remainTime)
        {

            foreach (var data in particleSystemDataArray)
                data.FadeOut(remainTime);

            foreach (var data in animationDataArray)
                data.FadeOut(remainTime);
            
            foreach (var data in trailRendererDataArray)
                data.FadeOut(remainTime);
        }
    }

}