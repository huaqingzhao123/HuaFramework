/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2021/1/13 16:48:43
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    [Serializable]
    public class PopTextStyleSetting
    {
        public PopTextStyleInfos styleInfos;

        [NonSerialized] protected Dictionary<string, PopTextStyleInfo> _cacheStyles;
        [NonSerialized] protected bool _isInited = false;

        public PopTextStyleInfo GetStyle(string name)
        {
            Initialize();
            return _cacheStyles[name];
        }

        private void Initialize()
        {
            if (_isInited)
            {
                return;
            }
            _isInited = true;

            _cacheStyles = new Dictionary<string, PopTextStyleInfo>();

            foreach (var item in styleInfos.styles)
            {
                _cacheStyles[item.name] = item;
            }
        }
    }

    public struct PopTextSetting
    {
        public Vector3 position { get; private set; }
        public string text { get; private set; }
        public PopTextStyleInfo styleInfo { get; private set; }
        public float size { get; private set; }
        public Color color { get; private set; }

        #region info

        public float lifeTime => styleInfo.lifeTime;

        public bool useAlphaOverLifeTime => styleInfo.useAlphaOverLifeTime;
        public AnimationCurve alphaOverLifeTime => styleInfo.alphaOverLifeTime;

        public bool useSizeOverLifeTime => styleInfo.useSizeOverLifeTime;
        public AnimationCurve sizeOverLifeTime => styleInfo.sizeOverLifeTime;

        public Vector3 velocity => styleInfo.velocity;
        public Vector3 gravity => styleInfo.gravity;
        public float damping => styleInfo.damping;

        #endregion info

        public Color GetColor(float progress)
        {
            Color target = color;
            if (useAlphaOverLifeTime)
            {
                target.a = alphaOverLifeTime.Evaluate(progress);
            }
            return target;
        }

        public float GetSize(float progress)
        {
            return (useSizeOverLifeTime ? sizeOverLifeTime.Evaluate(progress) : 1f) * size;
        }

        public PopTextSetting(PopTextStyleInfo style, Vector3 position, string text, float colorTime = 0, float sizeTime = 0)
        {
            this.position = position;
            this.text = text;
            this.styleInfo = style;
            this.size = style.sizeRange.Evaluate(sizeTime);
            this.color = style.colorRange.Evaluate(colorTime);
        }

        public override string ToString()
        {
            return $"PTS:Text={text},Pos={position},LifeTime={lifeTime}";
        }
    }

    /// <summary>
    /// PopNumber
    /// </summary>
    public class PopText : ResourceItem
    {
        [SerializeField]
        protected TextMesh _text;

        public UPopTextSystem system { get; set; }

        public PopTextSetting setting { get; protected set; }

        public float timer { get; private set; } = 0f;
        public bool isDie => timer >= setting.lifeTime;

        public float GetProgress() => Mathf.Clamp01(timer / setting.lifeTime);

        private Transform cameraTransfrom;
        public Vector3 velocity { get; private set; }

        public void Init(PopTextSetting setting)
        {
            cameraTransfrom = App.camera.cameraTransform;

            this.setting = setting;
            transform.position = setting.position;
            _text.text = setting.text;
            _text.color = setting.GetColor(0f);
            velocity = FixedVector(setting.velocity);
        }

        public Vector3 FixedVector(Vector3 dir)
        {
            var followObj = App.game.ucamera.followObj;
            if (followObj == null)
            {
                return dir;
            }

            return followObj.localToWorldMatrix.MultiplyVector((FPPhysics.Vector3)dir);
        }

        public override void OnPushPool()
        {
            base.OnPushPool();
            _text.text = "未初始化";
            system = null;
            timer = 0f;
            setting = default;
            cameraTransfrom = null;
            velocity = Vector3.zero;
        }

        public void OnUpdate(float deltaTime)
        {
            timer += deltaTime;

            float progress = this.GetProgress();
            float size = setting.GetSize(progress);
            Color color = setting.GetColor(progress);

            _text.color = color;
            _text.characterSize = size;

            Vector3 position = transform.position;
            var g = FixedVector(setting.gravity * deltaTime);
            var v = velocity;
            velocity = Vector3.Lerp(g + v, Vector3.zero, Mathf.Clamp01(setting.damping * deltaTime));
            transform.position = position + velocity * deltaTime;
        }
    }
}