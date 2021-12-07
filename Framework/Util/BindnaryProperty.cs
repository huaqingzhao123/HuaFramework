using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HuaFramework.Utility 
{
    public class BindnaryProperty<T> where T : IEquatable<T>
    {
        public UnityEvent<T> OnValueChanged;

        private T _value;
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!_value.Equals(value))
                {
                    _value = value;
                    OnValueChanged?.Invoke(_value);
                }
            }
        }


    }

}
