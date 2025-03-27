using UnityEngine;

namespace CoreToolkit.Runtime.Custom_Debug_Log.Scripts
{
    [CreateAssetMenu(fileName = "Custom Debug Settings", menuName = "Debug/Custom Debug Settings")]
    public class CustomDebugSettings : ScriptableObject
    {
        public bool showLog;
    }
}
