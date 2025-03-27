using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreToolkit.Runtime.Helpers
{
    [Serializable]
    public class SerializedCallback<TReturn> : ISerializationCallbackReceiver 
    {
        [SerializeField] Object targetObject;
        [SerializeField] string methodName;
        [SerializeField] AnyValue[] parameters;
    
        [NonSerialized] Delegate cachedDelegate;
        [NonSerialized] bool isDelegateRebuilt;

        public TReturn Invoke() {
            return Invoke(parameters);
        }

        public TReturn Invoke(params AnyValue[] args) {
            if (!isDelegateRebuilt) BuildDelegate();

            if (cachedDelegate != null) {
                var result = cachedDelegate.DynamicInvoke(ConvertParameters(args));
                return (TReturn)Convert.ChangeType(result, typeof(TReturn));
            }
            Debug.LogWarning($"Unable to invoke method {methodName} on {targetObject}");
            return default;
        }

        object[] ConvertParameters(AnyValue[] args) {
            if (args == null || args.Length == 0) return Array.Empty<object>();
        
            var convertedParams = new object[args.Length];
            for (int i = 0; i < args.Length; i++) {
                convertedParams[i] = args[i].ConvertValue<object>();
            }
            return convertedParams;
        }

        void BuildDelegate() {
            cachedDelegate = null;

            if (targetObject == null || string.IsNullOrEmpty(methodName)) {
                Debug.LogWarning("Target object or method name is null, cannot rebuild delegate.");
                return;
            }
        
            Type targetType = targetObject.GetType();
            MethodInfo methodInfo = targetType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo == null) {
                Debug.LogWarning($"Method {methodName} not found on {targetObject}");
                return;
            }
        
            Type[] parameterTypes = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
            if (parameters.Length != parameterTypes.Length) {
                Debug.LogWarning($"Parameter mismatch for method {methodName}");
                return;
            }
        
            Type delegateType = Expression.GetDelegateType(parameterTypes.Append(methodInfo.ReturnType).ToArray());
            cachedDelegate = methodInfo.CreateDelegate(delegateType, targetObject);
            isDelegateRebuilt = true;
        }
    
        public void OnBeforeSerialize() {
            // noop
        }
    
        public void OnAfterDeserialize() {
            isDelegateRebuilt = false;
        }
    }
}