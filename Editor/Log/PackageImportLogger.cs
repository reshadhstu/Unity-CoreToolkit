using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoreToolkit.Editor.Log
{
    [InitializeOnLoad]
    public static class PackageImportLogger
    {
        private static string PackageVersion => GetPackageVersion();
        private static string ImportKey => $"PackageImported_{PackageVersion}";

        static PackageImportLogger()
        {
            if (!EditorPrefs.HasKey(ImportKey))
            {
                Debug.Log($"Package Version {PackageVersion} Imported Successfully");
                EditorPrefs.SetBool(ImportKey, true);
            }
        }

        private static string GetPackageVersion()
        {
            // Construct the path to your package.json file
            string packageJsonPath = Path.Combine("Packages", "CoreToolkit", "package.json");

            // Load the package.json file as a TextAsset
            TextAsset packageJsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(packageJsonPath);

            if (packageJsonAsset != null)
            {
                // Parse the JSON content to extract the version
                string jsonText = packageJsonAsset.text;
                var jsonObject = JsonUtility.FromJson<PackageJson>(jsonText);
                return jsonObject.version;
            }
            else
            {
                Debug.LogWarning("Could not find package.json at " + packageJsonPath);
                return "Unknown";
            }
        }

        // Helper class to deserialize the version field from package.json
        [System.Serializable]
        private class PackageJson
        {
            public string version;
        }
    }
}