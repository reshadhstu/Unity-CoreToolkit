using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;

namespace CoreToolkit.Editor.Helpers
{
    [InitializeOnLoad]
    public static class SceneSwitcher
    {
        private static string[] _sceneNames = Array.Empty<string>();
        private static int _selectedIndex;
        private static string _lastActiveScene = "";
        private static VisualElement _toolbarUI;

        private static float _positionOffset = 180f; // Move closer to the Play button
        private static float _dropdownBoxHeight = 20f; // Dropdown button height

        private static bool FetchAllScenes
        {
            get => EditorPrefs.GetBool("SceneSwitcher_FetchAllScenes", false);
            set => EditorPrefs.SetBool("SceneSwitcher_FetchAllScenes", value);
        }

        static SceneSwitcher()
        {
            RefreshSceneList();
            SelectCurrentScene(); // Automatically select the open scene

            // Hook into the scene change events
            EditorSceneManager.activeSceneChangedInEditMode += (prev, current) => UpdateSceneSelection();
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            EditorApplication.delayCall += AddToolbarUI;
        }

        private static void AddToolbarUI()
        {
            var toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
            if (toolbarType == null) return;

            var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
            if (toolbars.Length == 0) return;

            var toolbar = toolbars[0];
            var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            if (rootField == null) return;

            var root = rootField.GetValue(toolbar) as VisualElement;
            if (root == null) return;

            var leftContainer = root.Q("ToolbarZoneLeftAlign");
            if (leftContainer == null) return;

            // Remove the old UI if it exists to prevent duplication
            if (_toolbarUI != null)
            {
                leftContainer.Remove(_toolbarUI);
            }

            _toolbarUI = new IMGUIContainer(OnGUI);
            _toolbarUI.style.marginLeft = _positionOffset;

            leftContainer.Add(_toolbarUI);
        }

        private static void OnGUI()
        {
            CheckAndRefreshScenes();

            if (_selectedIndex >= _sceneNames.Length)
                _selectedIndex = 0;

            bool isPlaying = EditorApplication.isPlaying; // Check if in Play Mode

            GUILayout.BeginHorizontal();

            // Fetch all scenes toggle button (Disabled in Play Mode)
            EditorGUI.BeginDisabledGroup(isPlaying);
            bool newFetchAllScenes = GUILayout.Toggle(FetchAllScenes, "All Scenes", "Button", GUILayout.Height(_dropdownBoxHeight));
            if (newFetchAllScenes != FetchAllScenes)
            {
                FetchAllScenes = newFetchAllScenes;
                RefreshSceneList();
                SelectCurrentScene();
            }
            EditorGUI.EndDisabledGroup();

            // Scene dropdown with the currently selected scene displayed (Disabled in Play Mode)
            EditorGUI.BeginDisabledGroup(isPlaying);
            GUIStyle popupStyle = new GUIStyle(EditorStyles.popup)
            {
                fixedHeight = _dropdownBoxHeight
            };

            int newIndex = EditorGUILayout.Popup(_selectedIndex, _sceneNames, popupStyle, GUILayout.Width(150), GUILayout.Height(_dropdownBoxHeight));

            if (newIndex != _selectedIndex)
            {
                _selectedIndex = newIndex;
                LoadScene(_sceneNames[_selectedIndex]);
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();
        }

        private static void RefreshSceneList()
        {
            if (FetchAllScenes)
            {
                _sceneNames = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories)
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToArray();
            }
            else
            {
                _sceneNames = EditorBuildSettings.scenes
                    .Where(scene => scene.enabled)
                    .Select(scene => Path.GetFileNameWithoutExtension(scene.path))
                    .ToArray();
            }
        }

        private static void CheckAndRefreshScenes()
        {
            string[] currentScenes;
            if (FetchAllScenes)
            {
                currentScenes = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories)
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToArray();
            }
            else
            {
                currentScenes = EditorBuildSettings.scenes
                    .Where(scene => scene.enabled)
                    .Select(scene => Path.GetFileNameWithoutExtension(scene.path))
                    .ToArray();
            }

            if (!currentScenes.SequenceEqual(_sceneNames))
            {
                _sceneNames = currentScenes;
                SelectCurrentScene();
            }
        }

        static void SelectCurrentScene()
        {
            string currentScene = Path.GetFileNameWithoutExtension(SceneManager.GetActiveScene().path);
            int index = System.Array.IndexOf(_sceneNames, currentScene);
            if (index != -1)
            {
                _selectedIndex = index;
                _lastActiveScene = currentScene;
            }
        }

        private static void UpdateSceneSelection()
        {
            string currentScene = Path.GetFileNameWithoutExtension(SceneManager.GetActiveScene().path);
            if (currentScene != _lastActiveScene)
            {
                _lastActiveScene = currentScene;
                SelectCurrentScene();
            }
        }

        private static void LoadScene(string sceneName)
        {
            string scenePath;

            if (FetchAllScenes)
            {
                scenePath = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories)
                    .FirstOrDefault(path => Path.GetFileNameWithoutExtension(path) == sceneName);
            }
            else
            {
                scenePath = EditorBuildSettings.scenes
                    .FirstOrDefault(scene => scene.enabled && scene.path.Contains(sceneName))?.path;
            }

            if (!string.IsNullOrEmpty(scenePath))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(scenePath);
                }
            }
            else
            {
                Debug.LogError("Scene not found: " + sceneName);
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state is PlayModeStateChange.EnteredPlayMode or PlayModeStateChange.ExitingPlayMode)
            {
                EditorApplication.delayCall += AddToolbarUI;
            }
        }
    }
}