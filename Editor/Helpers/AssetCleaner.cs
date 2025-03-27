using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoreToolkit.Editor.Helpers
{
    public class AssetCleaner : EditorWindow
    {
        [MenuItem("Tools/CoreToolkit/Asset Cleaner")]
        public static void ShowWindow()
        {
            GetWindow(typeof(AssetCleaner));
        }

        [Obsolete("Obsolete")]
        private void OnGUI()
        {
            GUILayout.Label("Asset Cleaner", EditorStyles.boldLabel);
            if (GUILayout.Button("Clean Unused Assets"))
            {
                // Show a confirmation dialog before proceeding
                bool confirmed = EditorUtility.DisplayDialog(
                    "Confirm Asset Cleaning",
                    "WARNING: This operation will permanently delete unused assets. " +
                    "Make sure that all necessary assets are backed up or that your project is under version control. " +
                    "Do you want to proceed?",
                    "Yes, Clean Assets",
                    "Cancel"
                );

                // If the user confirms, proceed with cleaning
                if (confirmed)
                {
                    CleanUnusedAssets();
                }
            }
        }

        [Obsolete("Obsolete")]
        private static void CleanUnusedAssets()
        {
            HashSet<string> usedAssets = new HashSet<string>();

            // Include assets used in scenes
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    string[] dependencies = AssetDatabase.GetDependencies(scene.path, true);
                    foreach (string dependency in dependencies)
                    {
                        usedAssets.Add(dependency);
                    }
                }
            }

            // Include assets used in Player Settings
            Texture2D[] playerIcons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);
            if (playerIcons.Length > 0)
            {
                Texture2D playerIcon = playerIcons[0];
                if (playerIcon != null)
                {
                    string playerIconPath = AssetDatabase.GetAssetPath(playerIcon);
                    if (!string.IsNullOrEmpty(playerIconPath))
                        usedAssets.Add(playerIconPath);
                }
            }

            // Include all prefabs in the project
            string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
            foreach (string prefabGuid in allPrefabs)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
                string[] dependencies = AssetDatabase.GetDependencies(path);
                foreach (string dependency in dependencies)
                    usedAssets.Add(dependency);
            }

            // Include all materials
            string[] allMaterials = AssetDatabase.FindAssets("t:Material");
            foreach (string materialGuid in allMaterials)
            {
                string path = AssetDatabase.GUIDToAssetPath(materialGuid);
                string[] dependencies = AssetDatabase.GetDependencies(path);
                foreach (string dependency in dependencies)
                    usedAssets.Add(dependency);
            }

            // Scan entire Assets folder
            string[] allAssets = Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories);
            List<string> unusedAssets = new List<string>();

            foreach (string asset in allAssets)
            {
                if (asset.EndsWith(".meta"))
                    continue;
                string assetPath = asset.Replace("\\", "/");
                if (!usedAssets.Contains(assetPath) && !assetPath.Contains("/Editor/"))
                {
                    unusedAssets.Add(assetPath);
                }
            }

            // Delete unused assets
            foreach (string unused in unusedAssets)
            {
                AssetDatabase.DeleteAsset(unused);
                Debug.Log("Deleted Unused Asset: " + unused);
            }

            AssetDatabase.Refresh();
            Debug.Log("Asset Cleaning Completed!");
        }
    }
}