using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreToolkit.Editor.Design.Hierarchy
{
    public class SetIconWindow : EditorWindow
    {
        // Use a defined (e.g., CORE_TOOLKIT_UPM) to decide the menu path.
// #if CORE_TOOLKIT_UPM
//         private const string MenuPathToAddIcon = "Tools/Script Icon/Set Script Icon";
//         private const string MenuPathToRemoveIcon = "Tools/Script Icon/Remove Script Icon";
// #else
//         private const string MenuPathToAddIcon = "Assets/Tools/Script Icon/Set Script Icon";
//         private const string MenuPathToRemoveIcon = "Assets/Tools/Script Icon/Remove Script Icon";
// #endif
        
        private const string MenuPathToAddIcon = "Assets/Tools/Script Icon/Set Script Icon";
        private const string MenuPathToRemoveIcon = "Assets/Tools/Script Icon/Remove Script Icon";
        
        private List<Texture2D> _icons;
        private int _selectedIcon;
        private Vector2 _scrollPos;

        [MenuItem(MenuPathToAddIcon, priority = 0)]
        public static void ShowMenuItem()
        {
            SetIconWindow window = (SetIconWindow)GetWindow(typeof(SetIconWindow));
            window.titleContent = new GUIContent("Set Script Icon");
            window.minSize = new Vector2(450, 350);
            window.Show();
        }

        [MenuItem(MenuPathToRemoveIcon, priority = 1)]
        public static void RemoveScriptIcon()
        {
            AssetDatabase.StartAssetEditing();
            foreach (Object asset in Selection.objects)
            {
                string assetPath = AssetDatabase.GetAssetPath(asset);
                MonoImporter monoImporter = AssetImporter.GetAtPath(assetPath) as MonoImporter;
                if (monoImporter) 
                    monoImporter.SetIcon(null);
                AssetDatabase.ImportAsset(assetPath);
            }
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        [MenuItem(MenuPathToAddIcon, validate = true)]
        public static bool ShowMenuItemValidation()
        {
            foreach (Object asset in Selection.objects)
            {
                if (asset.GetType() != typeof(MonoScript))
                    return false;
            }
            return true;
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Select an Icon", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // Load icons if not already loaded.
            if (_icons == null)
            {
                _icons = new List<Texture2D>();
                string[] assetGUIDs = AssetDatabase.FindAssets("t:texture2d l:ScriptIcon");
                foreach (string assetGuid in assetGUIDs)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                    if (tex != null)
                        _icons.Add(tex);
                }
            }

            // If no icons, show a message and Close button.
            if (_icons == null || _icons.Count == 0)
            {
                EditorGUILayout.HelpBox("No icons to display.", MessageType.Info);
                GUILayout.Space(10);
                if (GUILayout.Button("Close", GUILayout.Width(100)))
                {
                    Close();
                }
                return;
            }

            // Display the icons in a scrollable grid.
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(200));
            GUIStyle iconStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 80,
                fixedHeight = 80,
                margin = new RectOffset(5, 5, 5, 5)
            };
            Texture[] textureArray = _icons.ConvertAll<Texture>(icon => icon).ToArray();
            _selectedIcon = GUILayout.SelectionGrid(_selectedIcon, textureArray, 5, iconStyle);
            EditorGUILayout.EndScrollView();
            GUILayout.Space(10);

            // Show a preview of the selected icon.
            EditorGUILayout.LabelField("Selected Icon Preview:", EditorStyles.boldLabel);
            GUILayout.Space(5);
            GUILayout.Label(_icons[_selectedIcon], GUILayout.Width(80), GUILayout.Height(80));

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply", GUILayout.Width(100)))
            {
                ApplyScriptIcon(_icons[_selectedIcon]);
                Close();
            }
            if (GUILayout.Button("Cancel", GUILayout.Width(100)))
            {
                Close();
            }
            EditorGUILayout.EndHorizontal();

            // Optionally, handle keyboard events.
            if (Event.current != null && Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                {
                    ApplyScriptIcon(_icons[_selectedIcon]);
                    Close();
                }
                else if (Event.current.keyCode == KeyCode.Escape)
                {
                    Close();
                }
            }
        }

        private void ApplyScriptIcon(Texture2D icon)
        {
            AssetDatabase.StartAssetEditing();
            foreach (Object asset in Selection.objects)
            {
                string assetPath = AssetDatabase.GetAssetPath(asset);
                MonoImporter monoImporter = AssetImporter.GetAtPath(assetPath) as MonoImporter;
                if (monoImporter)
                    monoImporter.SetIcon(icon);
                AssetDatabase.ImportAsset(assetPath);
            }
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }
    }
}
