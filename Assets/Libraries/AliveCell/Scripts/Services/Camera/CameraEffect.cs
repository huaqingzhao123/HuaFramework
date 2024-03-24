using System;
using System.Collections.Generic;
//using AliveCell.CameraEffects;
using UnityEngine.Rendering.PostProcessing;

namespace AliveCell
{
    public class CameraEffect : ICameraOperation
    {
        public PostProcessVolume postProcess => target.cameraRoot.postProcess;
        public PostProcessProfile profile => postProcess.profile;
        private Dictionary<Type, ICameraEffect> type2Effect = new Dictionary<Type, ICameraEffect>();
        private List<ICameraEffect> effects = new List<ICameraEffect>();
        public CameraEffect(CameraService target) : base(target)
        {
            //AddEffect(new InjuredEffect(this));
        }

        public T GetEffect<T>() where T : ICameraEffect => type2Effect.TryGetValue(typeof(T), out var result) ? result as T : null;

        protected override void OnEnable()
        {
            base.OnEnable();
            foreach (var item in effects)
            {
                item.Initialize();
            }
        }
        protected override void OnDisable()
        {
            foreach (var item in effects)
            {
                item.Reset();
            }
            base.OnDisable();
        }
        public void AddEffect(ICameraEffect effect)
        {
            effects.Add(effect);
            type2Effect[effect.GetType()] = effect;
        }

        public override void Update(float deltaTime)
        {
            foreach (var effect in effects)
            {
                effect.Update(deltaTime);
            }
        }
    }

    public abstract class ICameraEffect
    {
        public CameraEffect effect { get; private set; }

        public CameraEffectSetting effectSetting => GlobalSetting.camera.effect;

        protected PostProcessVolume postProcess => effect.postProcess;
        protected PostProcessProfile profile => effect.profile;

        public ICameraEffect(CameraEffect effect)
        {
            this.effect = effect;
        }

        public abstract void Initialize();

        public abstract void Update(float deltaTime);

        public abstract void Reset();

    }

}