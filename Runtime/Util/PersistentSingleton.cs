using UnityEngine;

namespace CoreToolkit.Runtime.Util 
{
    public abstract class PersistentSingleton<T> : MonoBehaviour where T : Component 
    {
        public bool autoUnparentOnAwake = true;
        private static T _instance;
        public static T Instance 
        {
            get 
            {
                if (_instance == null) 
                {
                    _instance = FindAnyObjectByType<T>();
                    
                    if (_instance == null) 
                    {
                        var go = new GameObject(typeof(T).Name + " Auto-Generated");
                        _instance = go.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Make sure to call base.Awake() in override if you need awake.
        /// </summary>
        protected virtual void Awake() 
        {
            if (!Application.isPlaying) return;

            if (autoUnparentOnAwake) transform.SetParent(null);

            if (_instance == null) 
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            } 
            else if (_instance != this) 
            {
                Destroy(gameObject);
            }
        }
    }
}
