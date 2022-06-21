using HuaFramework.TypeEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HuaFramework.Utility
{
    public class BindableProperty<T>
    {
        public BindableProperty(T defaultValue = default)
        {
            _value = defaultValue;
        }
        private T _value;

        public T Value
        {
            get { return _value; }
            set
            {
                if (value == null && _value == null) return;
                if (value != null && _value.Equals(value)) return;
                _value = value;
                if (mOnValueChanged != null)
                    mOnValueChanged.Invoke(value);
            }
        }

        private Action<T> mOnValueChanged = (v) => { };

        public IUnRegister Register(Action<T> onValueChanged)
        {
            mOnValueChanged += onValueChanged;
            return new BindablePropertyUnRegister<T>()
            {
                BindableProperty = this,
                OnValueChanged = onValueChanged
            };
        }
        public IUnRegister RegisterWithInitValue(Action<T> onValueChanged)
        {
            onValueChanged(_value);
            return Register(onValueChanged);
        }

        /// <summary>
        /// 定义由BindableProperty<T> 到T的隐式转换运算符
        /// </summary>
        /// <param name="property"></param>
        public static implicit operator T(BindableProperty<T> property)
        {
            return property.Value;
        }
        public override string ToString()
        {
            return Value.ToString();
        }

        public void UnRegister(Action<T> onValueChanged)
        {
            mOnValueChanged -= onValueChanged;
        }
    }

    public class BindablePropertyUnRegister<T> : IUnRegister
    {
        public BindableProperty<T> BindableProperty { get; set; }

        public Action<T> OnValueChanged { get; set; }

        public void UnRegister()
        {
            BindableProperty.UnRegister(OnValueChanged);
        }
    }

}
