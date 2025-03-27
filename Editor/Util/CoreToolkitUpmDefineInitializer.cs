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
            foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (group == BuildTargetGroup.Unknown)
                    continue;

                try
                {
                    // Get current defines.
                    string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                    List<string> defineList = defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    bool updated = false;

                    if (!defineList.Contains(DefineSymbolUpm))
                    {
                        defineList.Add(DefineSymbolUpm);
                        Debug.Log($"Added {DefineSymbolUpm} to {group}");
                        updated = true;
                    }

                    if (updated)
                    {
                        string newDefines = string.Join(";", defineList.ToArray());
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, newDefines);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Could not update define symbols for group {group}: {ex.Message}");
                }
            }
        }
    }
}
