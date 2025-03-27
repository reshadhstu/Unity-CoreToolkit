#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CoreToolkit.Editor.Extensions
{
    /// <summary>
    /// A Unity editor extension for copying and pasting all components between GameObjects.
    /// The tool supports handling multiple components of the same type and correctly restores
    /// their data using the Unity Undo system. It provides menu items to copy components
    /// from the currently selected GameObject and paste them onto other selected GameObjects.
    /// </summary>
    public abstract class ComponentsCopier {
        private static Component[] _copiedComponents;
        private static Dictionary<Type, int> _componentCounters = new();

        [MenuItem("GameObject/TransferComponents/Copy %&C")]
        static void Copy() {
            var activeObject = Selection.activeGameObject;
            if (activeObject == null) return;

            _copiedComponents = activeObject.GetComponents<Component>();
            _componentCounters.Clear();
        }

        [MenuItem("GameObject/TransferComponents/Paste %&P")]
        static void Paste() {
            if (_copiedComponents == null) {
                Debug.LogError("Nothing copied!"); 
                return;
            }

            foreach (var target in Selection.gameObjects) {
                if (!target) continue;

                Undo.RegisterCompleteObjectUndo(target, $"{target.name}: Paste All Components");

                foreach (var copied in _copiedComponents) {
                    if (!copied) continue;

                    ComponentUtility.CopyComponent(copied);
                    Type componentType = copied.GetType();
                    var targetComponents = target.GetComponents(componentType);

                    if (targetComponents.Length > 0 && _componentCounters.ContainsKey(componentType)) {
                        _componentCounters[componentType]++;
                    } else {
                        _componentCounters[componentType] = 0;
                    }

                    int index = _componentCounters[componentType];
                    if (targetComponents.Length - index > 0) {
                        var targetComponent = targetComponents[index];
                        Undo.RecordObject(targetComponent, $"Paste {componentType} Values");
                        PasteComponent(() => ComponentUtility.PasteComponentValues(targetComponent), componentType);
                    } else {
                        PasteComponent(() => ComponentUtility.PasteComponentAsNew(target), componentType);
                    }
                }
            }

            _copiedComponents = null;
        }

        static void PasteComponent(Func<bool> pasteAction, Type componentType) {
            if (pasteAction()) {
                Debug.Log($"Successfully pasted: {componentType}");
            } else {
                Debug.LogError($"Failed to copy: {componentType}");
            }
        }
    }
}
#endif