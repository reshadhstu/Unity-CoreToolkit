using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CoreToolkit.Editor.Util
{
    public static class CoreToolkitUpmToggle
    {
        private const string DefineSymbol = "CORE_TOOLKIT_UPM";
        private const string MenuItemPath = "Tools/CoreToolkit/Toggle UPM";

        // This menu item toggles the define symbol on or off for all build target groups.
        [MenuItem(MenuItemPath, priority = 51)]
        [Obsolete("Obsolete")]
        public static void ToggleUpm()
        {
            // Loop through all valid build target groups.
            foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (group == BuildTargetGroup.Unknown)
                    continue;

                try
                {
                    // Get the current define symbols for the group.
                    string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                    List<string> defineList = defines
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                    if (defineList.Contains(DefineSymbol))
                    {
                        // Remove the symbol if it is present.
                        defineList.Remove(DefineSymbol);
                        Debug.Log($"Removed {DefineSymbol} from {group}");
                    }
                    else
                    {
                        // Add the symbol if it's not present.
                        defineList.Add(DefineSymbol);
                        Debug.Log($"Added {DefineSymbol} to {group}");
                    }

                    // Set the updated symbols back.
                    string newDefines = string.Join(";", defineList.ToArray());
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, newDefines);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Could not update define symbols for group {group}: {ex.Message}");
                }
            }
        }

        // This validation method updates the menu item checkmark based on the active build target group.
        [MenuItem(MenuItemPath, validate = true)]
        [Obsolete("Obsolete")]
        private static bool ToggleUpmValidate()
        {
            BuildTargetGroup activeGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(activeGroup);
            bool isDefined = defines
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Contains(DefineSymbol);
            Menu.SetChecked(MenuItemPath, isDefined);
            return true;
        }
    }
}