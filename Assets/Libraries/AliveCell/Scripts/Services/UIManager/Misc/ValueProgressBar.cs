/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2021/1/8 16:12:23
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// ValueProgressBar
    /// </summary>
    public class ValueProgressBar : UISubControl
    {
        [SerializeField]
        protected Image _smoothValueImg;

        [SerializeField]
        protected Image _valueImg;

        [SerializeField]
        protected Text _valueText;

        [SerializeField]
        protected float _smoothTime = 1f;

        [SerializeField]
        protected bool _isSmoothing = false;

        public bool isSmoothing => _isSmoothing;

        protected float _timer = 0;
        protected float _value = 0;
        protected float _maxValue = 0;
        protected Vector2 _smoothRange = Vector2.zero;

        public void Initialize(float maxValue, float value)
        {
            _maxValue = maxValue;
            _value = value;

            _timer = 0f;
            _isSmoothing = false;

            float progress = _value / _maxValue;
            _smoothValueImg.fillAmount = progress;
            _valueImg.fillAmount = progress;

            _smoothRange.x = progress;
            _smoothRange.y = progress;

            UpdateText();
        }

        public void SetValue(float value)
        {
            _smoothRange.x = _value > value ? _value : value;
            _smoothRange.y = value;
            _value = value;
            _valueImg.fillAmount = _value / _maxValue;
            _smoothValueImg.fillAmount = _smoothRange.x / _maxValue;
            _timer = 0f;
            _isSmoothing = true;
            UpdateText();
        }

        private void UpdateText()
        {
            _valueText.text = string.Format("<color=yellow>{0}</color>/{1}", _value, _maxValue);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (_isSmoothing)
            {
                _timer += Time.deltaTime;
                float scale = Mathf.Clamp01(_timer / _smoothTime);
                if (Mathf.Approximately(scale, 1.0f))
                {
                    scale = 1.0f;
                    _isSmoothing = false;
                }
                float smoothValue = Mathf.Lerp(_smoothRange.x, _smoothRange.y, scale) / _maxValue;
                _smoothValueImg.fillAmount = smoothValue;
            }
        }
    }
}