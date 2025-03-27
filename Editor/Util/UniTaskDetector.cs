using System;
using UnityEditor;
using UnityEngine;

namespace CoreToolkit.Editor.Util
{
    [InitializeOnLoad]
    public static class UniTaskDetector
    {
        [Obsolete("Obsolete")]
        static UniTaskDetector()
        {
            // Check if UniTask is installed by looking for a UniTask type
            var uniTaskType = Type.GetType("Cysharp.Threading.Tasks.UniTask, UniTask");
            var currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            if (uniTaskType != null && !currentDefines.Contains("CORE_TOOLKIT_UNITASK"))
            {
                // UniTask is installed, add the define symbol
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    EditorUserBuildSettings.selectedBuildTargetGroup,
                    currentDefines + ";CORE_TOOLKIT_UNITASK"
                );
                Debug.Log("UniTask detected. Added CORE_TOOLKIT_UNITASK to Scripting Define Symbols.");
            }
            else if (uniTaskType == null && currentDefines.Contains("CORE_TOOLKIT_UNITASK"))
            {
                // UniTask is not installed, remove the define symbol
                var updatedDefines = currentDefines.Replace("CORE_TOOLKIT_UNITASK", "").Replace(";;", ";").Trim(';');
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    EditorUserBuildSettings.selectedBuildTargetGroup,
                    updatedDefines
                );
                Debug.Log("UniTask not detected. Removed CORE_TOOLKIT_UNITASK from Scripting Define Symbols.");
            }
        }
    }
}