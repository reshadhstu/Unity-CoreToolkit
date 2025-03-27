using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreToolkit.Runtime.Attributes
{
    [Serializable]
    public class InterfaceReference<TInterface, TObject> where TObject : Object where TInterface : class 
    {
        [SerializeField, HideInInspector] TObject underlyingValue;

        public TInterface Value {
            get => underlyingValue switch {
                null => null,
                TInterface @interface => @interface,
                _ => throw new InvalidOperationException($"{underlyingValue} needs to implement interface {nameof(TInterface)}.")
            };
            set => underlyingValue = value switch {
                null => null,
                TObject newValue => newValue,
                _ => throw new ArgumentException($"{value} needs to be of type {typeof(TObject)}.", string.Empty)
            };
        }

        public TObject UnderlyingValue {
            get => underlyingValue;
            set => underlyingValue = value;
        }
    
        public InterfaceReference() { }
    
        public InterfaceReference(TObject target) => underlyingValue = target;
    
        public InterfaceReference(TInterface @interface) => underlyingValue = @interface as TObject;
    
        public static implicit operator TInterface(InterfaceReference<TInterface, TObject> obj) => obj.Value;
    }

    [Serializable]
    public class InterfaceReference<TInterface> : InterfaceReference<TInterface, Object> where TInterface : class { }
}