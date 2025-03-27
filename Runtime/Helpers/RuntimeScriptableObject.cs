using System.Collections.Generic;
using UnityEngine;

namespace CoreToolkit.Runtime.Helpers
{
    public abstract class RuntimeScriptableObject : ScriptableObject
    {
        static readonly List<RuntimeScriptableObject> Instances = new();

        void OnEnable() => Instances.Add(this);
        void OnDisable() => Instances.Remove(this);
        
        protected abstract void OnRest();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetAllInstances()
        {
            foreach (var instance in Instances)
            {
                instance.OnRest();
            }
        }
    }
}
