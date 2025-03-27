using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CoreToolkit.Editor.Helpers
{
    [InitializeOnLoad]
    public static class PersistentPlayModeChanges
    {
        // Toggle flag, default = off
        private static bool s_isEnabled = false;
        private static bool s_isPlaying = false;

        // Keep track of modified objects
        private static Dictionary<int, TransformData> _modifiedObjects = new();

        static PersistentPlayModeChanges()
        {
            // Load user preference for whether the script is enabled
            s_isEnabled = EditorPrefs.GetBool("PersistentPlayModeChanges_Enabled", false);

            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update += TrackLiveChanges;
        }

        // -------------------------------------------------------------------------
        //  Menu Items: On / Off
        // -------------------------------------------------------------------------
        [MenuItem("Tools/PlayModeChanges/Transform/On", false, 41)]
        private static void TurnOnPlayModeChanges()
        {
            s_isEnabled = true;
            EditorPrefs.SetBool("PersistentPlayModeChanges_Enabled", true);
        }

        [MenuItem("Tools/PlayModeChanges/Transform/On", true, 42)]
        private static bool ValidateTurnOnPlayModeChanges()
        {
            // Show a checkmark if currently enabled
            Menu.SetChecked("Tools/PlayModeChanges/Transform/On", s_isEnabled);
            // Only enable this menu if currently off
            return !s_isEnabled;
        }

        [MenuItem("Tools/PlayModeChanges/Transform/Off", false, 43)]
        private static void TurnOffPlayModeChanges()
        {
            s_isEnabled = false;
            EditorPrefs.SetBool("PersistentPlayModeChanges_Enabled", false);
        }

        [MenuItem("Tools/PlayModeChanges/Transform/Off", true, 44)]
        private static bool ValidateTurnOffPlayModeChanges()
        {
            // Show a checkmark if currently disabled
            Menu.SetChecked("Tools/PlayModeChanges/Transform/Off", !s_isEnabled);
            // Only enable this menu if currently on
            return s_isEnabled;
        }

        // -------------------------------------------------------------------------
        //  Play Mode logic
        // -------------------------------------------------------------------------
        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            // If script is disabled, do nothing
            if (!s_isEnabled) return;

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                s_isPlaying = true;
                _modifiedObjects.Clear();
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                s_isPlaying = false;
                SaveChangesToEditorPrefs();

                // Use delayCall to mark the scene dirty after exiting play mode.
                EditorApplication.delayCall += () =>
                {
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                };
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                RestoreChangesFromEditorPrefs();
            }
        }

        private static void TrackLiveChanges()
        {
            // If script is disabled or not playing, do nothing
            if (!s_isEnabled || !s_isPlaying) return;

            // Use the new API: Object.FindObjectsByType with FindObjectsSortMode.None
            foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                // Check for missing scripts by iterating through all components
                bool hasMissingScript = false;
                Component[] comps = obj.GetComponents<Component>();
                foreach (var comp in comps)
                {
                    if (comp == null)
                    {
                        hasMissingScript = true;
                        break;
                    }
                }
                if (hasMissingScript)
                    continue;

                if (obj.scene.IsValid() && obj.transform.hasChanged)
                {
                    int id = obj.GetInstanceID();
                    if (!_modifiedObjects.ContainsKey(id))
                    {
                        _modifiedObjects[id] = new TransformData(obj.transform);
                    }
                    else
                    {
                        _modifiedObjects[id].Update(obj.transform);
                    }
                    obj.transform.hasChanged = false;
                }
            }
        }

        // -------------------------------------------------------------------------
        //  Saving and restoring transform data
        // -------------------------------------------------------------------------
        private static void SaveChangesToEditorPrefs()
        {
            foreach (var entry in _modifiedObjects)
            {
                string key = "TransformData_" + entry.Key;
                EditorPrefs.SetString(key, entry.Value.ToJson());
            }
        }

        private static void RestoreChangesFromEditorPrefs()
        {
            // Use the new API here as well
            foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                int id = obj.GetInstanceID();
                string key = "TransformData_" + id;

                if (EditorPrefs.HasKey(key))
                {
                    TransformData data = TransformData.FromJson(EditorPrefs.GetString(key));
                    if (data != null)
                    {
                        Undo.RecordObject(obj.transform, "Restore Play Mode Transform");
                        data.ApplyTo(obj.transform);
                        EditorUtility.SetDirty(obj.transform);
                    }
                    EditorPrefs.DeleteKey(key);
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("✅ Play Mode changes restored instantly with no delay!");
        }

        // -------------------------------------------------------------------------
        //  TransformData class
        // -------------------------------------------------------------------------
        private class TransformData
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;

            public TransformData(Transform transform)
            {
                Update(transform);
            }

            public void Update(Transform transform)
            {
                position = transform.position;
                rotation = transform.rotation;
                scale = transform.localScale;
            }

            public void ApplyTo(Transform transform)
            {
                transform.position = position;
                transform.rotation = rotation;
                transform.localScale = scale;
            }

            public string ToJson()
            {
                return JsonUtility.ToJson(this);
            }

            public static TransformData FromJson(string json)
            {
                return JsonUtility.FromJson<TransformData>(json);
            }
        }
    }
}