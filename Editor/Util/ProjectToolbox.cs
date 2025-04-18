using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CoreToolkit.Editor.Util
{
    public class ProjectToolbox : EditorWindow
    {
        private Vector2 _scrollPosition;
        private int _selectedTab;
        private string[] _tabs = { "Scenes", "Prefabs", "ScriptableObjects", "Tools", "Utilities" };

        // Search filters
        private string _prefabSearch = "";
        private string _scriptableSearch = "";

        // Cached asset paths
        private List<string> _scenePaths;
        private List<string> _prefabPaths;
        private List<string> _scriptablePaths;

        [MenuItem("Tools/CoreToolkit/Project Toolbox")]
        public static void ShowWindow()
        {
            GetWindow<ProjectToolbox>("Project Toolbox");
        }

        private void OnEnable()
        {
            RefreshPaths();
        }

        private void RefreshPaths()
        {
            // Scenes: exclude package scenes
            _scenePaths = AssetDatabase.FindAssets("t:Scene")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.StartsWith("Assets/"))
                .ToList();

            // Prefabs: only project Assets, exclude package prefabs
            _prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.StartsWith("Assets/"))
                .ToList();

            // ScriptableObjects: only from a specific folder (e.g., ScriptableObjects folder under Assets)
            _scriptablePaths = AssetDatabase.FindAssets("t:ScriptableObject")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.StartsWith("Assets/_Project/"))
                .ToList();
        }

        private void OnGUI()
        {
            // Tabs
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabs);
            EditorGUILayout.Space();

            // Refresh button
            if (GUILayout.Button("Refresh Asset Lists"))
            {
                RefreshPaths();
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            switch (_selectedTab)
            {
                case 0: DrawSceneTab(); break;
                case 1: DrawPrefabTab(); break;
                case 2: DrawScriptableTab(); break;
                case 3: DrawToolsTab(); break;
                case 4: DrawUtilityTab(); break;
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawSceneTab()
        {
            EditorGUILayout.LabelField("Manage Project Scenes", EditorStyles.boldLabel);

            for (int i = 0; i < _scenePaths.Count; i++)
            {
                string path = _scenePaths[i];
                string name = Path.GetFileNameWithoutExtension(path);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(name, GUILayout.MaxWidth(200));

                if (GUILayout.Button("↑", GUILayout.Width(25)) && i > 0)
                    (_scenePaths[i - 1], _scenePaths[i]) = (_scenePaths[i], _scenePaths[i - 1]);

                if (GUILayout.Button("↓", GUILayout.Width(25)) && i < _scenePaths.Count - 1)
                    (_scenePaths[i + 1], _scenePaths[i]) = (_scenePaths[i], _scenePaths[i + 1]);

                if (GUILayout.Button("Open", GUILayout.Width(50)))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        EditorSceneManager.OpenScene(path);
                }

                if (GUILayout.Button("Additive", GUILayout.Width(70)))
                {
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                }
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Add Ordered Scenes To Build Settings"))
            {
                var scenes = _scenePaths.Select(p => new EditorBuildSettingsScene(p, true)).ToArray();
                EditorBuildSettings.scenes = scenes;
                Debug.Log("Added " + scenes.Length + " scenes to Build Settings.");
            }
        }

        private void DrawPrefabTab()
        {
            EditorGUILayout.LabelField("All Prefabs in Project", EditorStyles.boldLabel);
            _prefabSearch = EditorGUILayout.TextField("Search Prefabs", _prefabSearch);

            foreach (var path in _prefabPaths)
            {
                string name = Path.GetFileNameWithoutExtension(path);
                if (!string.IsNullOrEmpty(_prefabSearch) && !name.ToLower().Contains(_prefabSearch.ToLower()))
                    continue;

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(prefab, typeof(GameObject), false);
                if (GUILayout.Button("Open", GUILayout.Width(50)))
                    AssetDatabase.OpenAsset(prefab);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawScriptableTab()
        {
            EditorGUILayout.LabelField("Project ScriptableObjects (Assets/_Project/)", EditorStyles.boldLabel);
            _scriptableSearch = EditorGUILayout.TextField("Search ScriptableObjects", _scriptableSearch);

            foreach (var path in _scriptablePaths)
            {
                string name = Path.GetFileNameWithoutExtension(path);
                if (!string.IsNullOrEmpty(_scriptableSearch) && !name.ToLower().Contains(_scriptableSearch.ToLower()))
                    continue;

                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null) continue;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(asset, asset.GetType(), false);
                if (GUILayout.Button("Select", GUILayout.Width(50)))
                    Selection.activeObject = asset;
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawToolsTab()
        {
            EditorGUILayout.LabelField("Quick Tools", EditorStyles.boldLabel);
            if (GUILayout.Button("Clear PlayerPrefs"))
            {
                PlayerPrefs.DeleteAll(); Debug.Log("PlayerPrefs Cleared");
            }
            if (GUILayout.Button("Force Recompile"))
            {
                AssetDatabase.Refresh(); Debug.Log("Forced Recompile");
            }
            if (GUILayout.Button("Rebake Lighting"))
            {
                Lightmapping.BakeAsync(); Debug.Log("Lighting rebake started");
            }
        }

        private void DrawUtilityTab()
        {
            EditorGUILayout.LabelField("Utility Shortcuts", EditorStyles.boldLabel);
            if (GUILayout.Button("Open Persistent Data Path"))
                EditorUtility.RevealInFinder(Application.persistentDataPath);
            if (GUILayout.Button("Take Screenshot in Game View"))
            {
                string path = Path.Combine(Application.dataPath, "../Screenshot_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");
                ScreenCapture.CaptureScreenshot(path);
                Debug.Log("Screenshot saved: " + path);
            }
        }
    }
}
