using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CoreToolkit.Editor.Helpers
{
    public class MissingScriptsEditor : EditorWindow
    {
        private List<GameObject> _objectsWithMissingScripts = new();
        private bool _foundMissingScripts = false;

        [MenuItem("Tools/Missing Scripts Cleaner")]
        public static void ShowWindow()
        {
            GetWindow<MissingScriptsEditor>("Missing Scripts Cleaner");
        }

        [Obsolete("Obsolete")]
        void OnGUI()
        {
            GUILayout.Label("Missing Scripts Cleaner", EditorStyles.boldLabel);

            if (GUILayout.Button("Find Missing Scripts in Scene"))
            {
                FindMissingScripts();
            }

            if (_foundMissingScripts && _objectsWithMissingScripts.Count > 0)
            {
                GUILayout.Label("GameObjects with Missing Scripts:");
                foreach (var obj in _objectsWithMissingScripts)
                {
                    EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                }

                if (GUILayout.Button("Remove All Missing Scripts"))
                {
                    RemoveMissingScripts();
                }
            }
        }

        [Obsolete("Obsolete")]
        void FindMissingScripts()
        {
            _objectsWithMissingScripts.Clear();
            GameObject[] allObjects = FindObjectsOfType<GameObject>();

            foreach (GameObject obj in allObjects)
            {
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(obj) > 0)
                {
                    _objectsWithMissingScripts.Add(obj);
                }
            }

            _foundMissingScripts = _objectsWithMissingScripts.Count > 0;
        }

        void RemoveMissingScripts()
        {
            foreach (GameObject obj in _objectsWithMissingScripts)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            }

            _objectsWithMissingScripts.Clear();
            _foundMissingScripts = false;
            EditorUtility.DisplayDialog("Cleanup Complete", "All missing scripts have been removed from the scene.", "OK");
        }
    }
}
