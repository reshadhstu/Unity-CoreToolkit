using UnityEditor;
using UnityEngine;

namespace CoreToolkit.Editor.Log
{
    [InitializeOnLoad]
    public static class PackageImportLogger
    {
        private const string ImportKey = "PackageImported";

        static PackageImportLogger()
        {
            if (!EditorPrefs.HasKey(ImportKey))
            {
                Debug.Log("Package Imported Successfully");
                EditorPrefs.SetBool(ImportKey, true);
            }
        }
    }
}