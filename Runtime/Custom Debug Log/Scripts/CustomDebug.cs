using UnityEngine;

namespace CoreToolkit.Runtime.Custom_Debug_Log.Scripts
{
    public static class CustomDebug
    {
        private static CustomDebugSettings _customDebugSettings;

        [RuntimeInitializeOnLoadMethod]
        static void LoadSettings()
        {
            _customDebugSettings = Resources.Load<CustomDebugSettings>("Custom Debug/Custom Debug Settings");
            
            if (!_customDebugSettings)
            {
                Debug.LogError("Custom Debug Log Settings Not Found. Please Ensure That, The Resource Path Is Correct");
            }
        }

        public static void Log(string message)
        {
            if (!_customDebugSettings)
            {
                Debug.LogError("Custom Debug Log Settings Not Found. Please Ensure That, The Resource Path Is Correct");
                return;
            }
            
            if (_customDebugSettings.showLog)
            {
                Debug.Log(message % CustomDebugColorize.Green % CustomDebugFontFormat.Bold );
            }
        }
        
        public static void LogWarning(string message)
        {
            if (!_customDebugSettings)
            {
                Debug.LogError("Custom Debug Log Settings Not Found. Please Ensure That, The Resource Path Is Correct");
                return;
            }

            if (_customDebugSettings.showLog)
            {
                Debug.Log(message % CustomDebugColorize.Yellow % CustomDebugFontFormat.Bold );
            }
        }

        public static void LogError(string message)
        {
            if (!_customDebugSettings)
            {
                Debug.LogError("Custom Debug Log Settings Not Found. Please Ensure That, The Resource Path Is Correct");
                return;
            }

            if (_customDebugSettings.showLog)
            {
                Debug.LogError(message % CustomDebugColorize.RedNice % CustomDebugFontFormat.Bold);
            }
        }
    }
}
