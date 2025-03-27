using System.Collections.Generic;
using CoreToolkit.Runtime.Helpers;
using UnityEngine;
using UnityEngine.Events;

namespace CoreToolkit.Runtime.SO
{
    public abstract class ScriptableVariable<T> : RuntimeScriptableObject
    {
        [SerializeField] private T _initialValue;
        [SerializeField] private T _value;

        public event UnityAction<T> OnValueChanged = delegate { };

        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value)) return;
                _value = value;
                OnValueChanged.Invoke(_value);
            }
        }

        protected override void OnRest() => OnValueChanged.Invoke(_value = _initialValue);
    }
}