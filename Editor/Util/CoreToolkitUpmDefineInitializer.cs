using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CoreToolkit.Editor.Util
{
    // Automatically execute when the editor loads.
    [InitializeOnLoad]
    public static class CoreToolkitUpmDefineInitializer
    {
        private const string DefineSymbolUpm = "CORE_TOOLKIT_ICONS";

        // This static constructor runs when scripts are reloaded.
        [Obsolete("Obsolete")]
        static CoreToolkitUpmDefineInitializer()
        {
            AddDefineSymbolsForAllBuildTargetGroups();
        }

        [Obsolete("Obsolete")]
        private static void AddDefineSymbolsForAllBuildTargetGroups()
        {
            bool anyUpdated = false; // Track if any changes were made
            foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                // Skip invalid or obsolete groups
                if (group == BuildTargetGroup.Unknown) continue;
                var fieldInfo = typeof(BuildTargetGroup).GetField(group.ToString());
                if (fieldInfo != null && Attribute.IsDefined(fieldInfo, typeof(ObsoleteAttribute))) continue;

                try
                {
                    string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                    List<string> defineList = defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    if (!defineList.Contains(DefineSymbolUpm))
                    {
                        defineList.Add(DefineSymbolUpm);
                        anyUpdated = true; // Mark that we made a change
                    }

                    string newDefines = string.Join(";", defineList.ToArray());
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, newDefines);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Could not update define symbols for group {group}: {ex.Message}");
                }
            }

            // Log only once if any updates occurred
            if (anyUpdated)
            {
                Debug.Log($"Added {DefineSymbolUpm} to all applicable build target groups.");
            }
        }
    }
}