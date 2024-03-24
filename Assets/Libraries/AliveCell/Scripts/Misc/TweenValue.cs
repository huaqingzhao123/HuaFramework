using System.Collections.Generic;
using FPPhysics;
using UnityEngine;
using XMLib;

using Vector3 = FPPhysics.Vector3;

namespace AliveCell
{
    public class TweenValueHelper
    {
        private List<ITweenValue> tweens = new List<ITweenValue>();
        
        public T Create<T>() where T : ITweenValue, new()
        {
            T result = new T();
            Add(result);
            return result;
        }
        public void Add<T>(T item) where T : ITweenValue
        {
            tweens.Add(item);
        }
        public void Clear()
        {
            tweens.Clear();
        }
        public void Update(float deltaTime)
        {
            foreach (var tween in tweens)
            {
                tween.Update(deltaTime);
            }
        }
        
    }
    public abstract class ITweenValue
    {
        public abstract void Update(float deltaTime);
    }

    public abstract class ITweenValue<T> : ITweenValue
    {
        public float time { get; protected set; } = 0f;
        public float duration { get; protected set; } = 0f;
        public Ease ease { get; protected set; } = 0f;
        public bool tweening { get; protected set; } = false;

        public T value { get; protected set; }
        public T fromValue { get; protected set; }
        public T toValue { get; protected set; }

        public AnimationCurve animationCurve;
        public virtual void SetValue(T value)
        {
            this.value = value;
            this.fromValue = value;
            this.toValue = value;

            this.duration = 0f;
            this.time = 0f;
            this.ease = Ease.Unset;
            this.tweening = false;
            this.animationCurve = null;
        }

        public virtual void SetValue(T value, float duration, Ease ease = Ease.Linear)
        {
            this.fromValue = this.value;
            this.toValue = value;

            this.duration = duration;
            this.time = 0f;
            this.ease = ease;
            this.tweening = true;
            this.animationCurve = null;
        }
        public virtual void SetValue(T value, float duration, AnimationCurve curve)
        {
            this.fromValue = this.value;
            this.toValue = value;

            this.duration = duration;
            this.time = 0f;
            this.ease = Ease.Linear;
            this.tweening = true;
            this.animationCurve = curve;
        }
        public void SetValue(T startValue, T endValue, float duration, Ease ease = Ease.Linear)
        {
            this.value = startValue;
            SetValue(endValue, duration, ease);
        }
        public void SetValue(T startValue, T endValue, float duration, AnimationCurve curve)
        {
            this.value = startValue;
            SetValue(endValue, duration, curve);
        }
        public virtual void SetToValue(T value)
        {
            this.toValue = value;
        }
        public override void Update(float deltaTime)
        {
            if (!tweening)
            {
                return;
            }

            time += deltaTime;
            float progress = 0;
            if (animationCurve != null)
            {
                //float _remappedTimeSinceStart = MMFeedbacksHelpers.Remap(time, 0f, duration, 0f, 1f);
                //float curveValue = animationCurve.Evaluate(_remappedTimeSinceStart);
                //progress = curveValue; //MMFeedbacksHelpers.Remap(curveValue, 0f, 1, 0, 1);
            }
            else
            {
                progress = Mathf.Clamp01(EaseUtility.Evaluate(ease, time, duration));
            }
            //float progress = Mathf.Clamp01(EaseUtility.Evaluate(ease, time, duration));
            if (Mathf.Approximately(progress, 1f))
            {
                progress = 1f;
                tweening = false;
            }
            UpdateValue(progress);
        }

        protected abstract void UpdateValue(float progress);
    }
    public class FloatTween : ITweenValue<float>
    {
        protected override void UpdateValue(float progress)
        {
            value = Mathf.Lerp(fromValue, toValue, progress);
        }

        public static implicit operator float(FloatTween v) => v.value;
    }
    public class Vector3Tween : ITweenValue<Vector3>
    {
        protected override void UpdateValue(float progress)
        {
            value = Vector3.Lerp(fromValue, toValue, (Fix64)progress);
            // tempV.x = Mathf.Lerp(fromValue.x, toValue.x, progress);
            // tempV.y = Mathf.Lerp(fromValue.x, toValue.x, progress);
            // tempV.z = Mathf.Lerp(fromValue.z, toValue.z, progress);

        }

        public static implicit operator Vector3(Vector3Tween v) => v.value;
    }
}