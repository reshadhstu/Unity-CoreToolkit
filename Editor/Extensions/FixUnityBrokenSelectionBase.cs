using System.Collections.Generic;
using CoreToolkit.Runtime.Attributes;
using UnityEditor;
using UnityEngine;

// Put this in an Editor folder
namespace CoreToolkit.Editor.Extensions
{
    [InitializeOnLoad]
    public class FixUnityBrokenSelectionBase : UnityEditor.Editor {
        static List<Object> newSelection;
        static Object[] lastSelection = { };

        static FixUnityBrokenSelectionBase() {
            // Subscribe to selection changed events
            Selection.selectionChanged += OnSelectionChanged;
            // Use update event to modify selections after changes
            EditorApplication.update += OnSceneUpdate;
        }

        static void OnSelectionChanged() {
            // Check if the mouse is over the SceneView
            var editorWindow = EditorWindow.mouseOverWindow;
            if (editorWindow == null) return;

            // Only modify user selection if selected from the SceneView
            if (editorWindow.GetType() != typeof(SceneView)) return;

            // Look through the current selection
            var futureSelection = new List<Object>();
            var changed = false;

            foreach (var go in Selection.GetFiltered<GameObject>(SelectionMode.Unfiltered)) {
                changed |= AdjustIfNeeded(go, futureSelection);
            }

            // If nothing has changed, set the future selection to null
            if (!changed) {
                futureSelection = null;
            }

            // Atomically update newSelection
            newSelection = futureSelection;

            // Remember this selection to compare with the next selection
            lastSelection = Selection.objects;
        }

        static bool AdjustIfNeeded(GameObject go, List<Object> futureSelection) {
            var parentWithGlobalSelectionBase = ParentWithGlobalSelectionBase(go);
            if (parentWithGlobalSelectionBase != null) {
                // Replace selection with GlobalSelectionBase parent
                futureSelection.Add(parentWithGlobalSelectionBase.gameObject);
                // Debug.Log("Selecting parent: " + parentWithGlobalSelectionBase.name);
                return true;
            }

            // Add the original GameObject
            futureSelection.Add(go);
            // Debug.Log("Selecting original object: " + go.name);
            return false;
        }

        static void OnSceneUpdate() {
            if (newSelection != null) {
                Selection.objects = newSelection.ToArray();
                newSelection = null;
            }
        }

        static GameObject ParentWithGlobalSelectionBase(GameObject go) {
            if (go.transform.parent == null) return null;

            foreach (MonoBehaviour component in go.transform.parent.GetComponentsInParent<MonoBehaviour>(false)) {
                if (component.GetType().GetCustomAttributes(typeof(SelectionBaseFixedAttribute), true).Length > 0) {
                    return component.gameObject;
                }
            }

            return null;
        }
    }
}