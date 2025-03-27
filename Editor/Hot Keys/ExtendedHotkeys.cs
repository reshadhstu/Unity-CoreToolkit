using UnityEditor;
using UnityEngine;

namespace CoreToolkit.Editor.Hot_Keys
{
    public static class ExtendedHotkeys
    {
        // Priority 10 for the first item
        [MenuItem("Tools/CoreToolkit/Select GameConfig %_T", false, 0)]
        static void SelectGameConfig()
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/_Project/Scripts/Runtime/SO/GameConfig.asset");
        }

        // Priority 21 for the second item (gap of 11 creates a separator)
        [MenuItem("Tools/CoreToolkit/Select GameManager %_G", false, 1)]
        static void SelectGameManager()
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/_Project/Scripts/Runtime/Managers/GameManager.cs");
        }

        [MenuItem("Tools/CoreToolkit/Toggle Inspector Lock _F3", false, 2)]
        private static void ToggleEditorWindowLock()
        {
            ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
            ActiveEditorTracker.sharedTracker.ForceRebuild();
        }
        
        [MenuItem("Tools/CoreToolkit/Toggle Enter Play Mode Options _F4", false, 3)]
        private static void ToggleEnterPlayModeOptions()
        {
            // Check the current state of the options.
            if (EditorSettings.enterPlayModeOptionsEnabled)
            {
                // Disable custom options to force full domain and scene reload.
                EditorSettings.enterPlayModeOptionsEnabled = false;
                Debug.Log("Enter Play Mode Options: Disabled (domain and scene will reload).");
            }
            else
            {
                // Enable custom options: no domain and scene reload.
                EditorSettings.enterPlayModeOptions = 
                    EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
                EditorSettings.enterPlayModeOptionsEnabled = true;
                Debug.Log("Enter Play Mode Options: Enabled (domain and scene will NOT reload).");
            }
        }
    }
}