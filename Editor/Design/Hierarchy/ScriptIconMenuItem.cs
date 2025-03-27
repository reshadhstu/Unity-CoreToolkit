using UnityEditor;
using UnityEngine;

namespace CoreToolkit.Editor.Design.Hierarchy
{
    public static class ScriptIconMenuItem
    {
        private const string Label = "ScriptIcon";
        
        [MenuItem("Tools/CoreToolkit/Script Icon Labels/Assign Script Icon Label")]
        public static void AssignScriptIconMenuItem()
        {
            Object[] objects = Selection.objects;
            
            if(objects == null) return;

            foreach (var obj in objects)
            {
                string[] labels = AssetDatabase.GetLabels(obj);
                
                if(!ArrayUtility.Contains(labels, Label))
                {
                    ArrayUtility.Add(ref labels, Label);
                    AssetDatabase.SetLabels(obj, labels);
                }
            }
        }
        
        [MenuItem("Tools/CoreToolkit/Script Icon Labels/Remove Script Icon Label")]
        public static void RemoveScriptIconMenuItem()
        {
            Object[] objects = Selection.objects;
            
            if(objects == null) return;

            foreach (var obj in objects)
            {
                string[] labels = AssetDatabase.GetLabels(obj);
                ArrayUtility.Remove(ref labels, Label);
                AssetDatabase.SetLabels(obj, labels);
            }
        }
        
        [MenuItem("Tools/CoreToolkit/Script Icon Labels/Find the Script Icons")]
        public static void FindScriptIconMenuItem()
        {
            string[] assetGUIDs = AssetDatabase.FindAssets($"t:texture2d l:{Label}");

            foreach (var assetGuid in assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGuid);
                Debug.Log(path);
            }
        }
    }
}
