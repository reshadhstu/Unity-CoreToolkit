using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreToolkit.Runtime.Attributes;
using UnityEditor;
using UnityEngine;

namespace CoreToolkit.Editor.GUIDrawer
{
    [InitializeOnLoad]
    public static class HierarchyIconDrawer 
    {
#if CORE_TOOLKIT_UPM
        private const string RequiredIconPath = "Packages/com.reshad.coretoolkit/Editor/Icons/RequiredIcon/RequiredIcon.png";
#else
        private const string RequiredIconPath = "Assets/CoreToolkit/Editor/Icons/RequiredIcon/RequiredIcon.png";
#endif
        
        private static readonly Texture2D RequiredIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(RequiredIconPath);

        private static readonly Dictionary<Type, FieldInfo[]> CachedFieldInfo = new ();

        static HierarchyIconDrawer() {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect) {
            if (EditorUtility.InstanceIDToObject(instanceID) is not GameObject gameObject) return;

            // Use GetComponentsInChildren to include components on children
            foreach (var component in gameObject.GetComponentsInChildren<Component>()) {
                if (component == null) continue;
            
                var fields = GetCachedFieldsWithRequiredAttribute(component.GetType());
                if (fields == null) continue;

                if (fields.Any(field => IsFieldUnassigned(field.GetValue(component)))) {
                    var iconRect = new Rect(selectionRect.xMax - 20, selectionRect.y, 16, 16);
                    GUI.Label(iconRect, new GUIContent(RequiredIcon, "One or more required fields are missing or empty."));
                    break;
                }
            }
        }

        static FieldInfo[] GetCachedFieldsWithRequiredAttribute(Type componentType) {
            if (!CachedFieldInfo.TryGetValue(componentType, out FieldInfo[] fields)) {
                fields = componentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                List<FieldInfo> requiredFields = new ();

                foreach (FieldInfo field in fields) {
                    bool isSerialized = field.IsPublic || field.IsDefined(typeof(SerializeField), false);
                    bool isRequired = field.IsDefined(typeof(RequiredFieldAttribute), false);

                    if (isSerialized && isRequired) {
                        requiredFields.Add(field);
                    }
                }
            
                fields = requiredFields.ToArray();
                CachedFieldInfo[componentType] = fields;
            }
            return fields;
        }

        static bool IsFieldUnassigned(object fieldValue) {
            if (fieldValue == null) return true;
        
            if (fieldValue is string stringValue && string.IsNullOrEmpty(stringValue)) return true;

            if (fieldValue is System.Collections.IEnumerable enumerable) {
                foreach (var item in enumerable) {
                    if (item == null) return true;
                }
            }
        
            return false;
        }
    }
}