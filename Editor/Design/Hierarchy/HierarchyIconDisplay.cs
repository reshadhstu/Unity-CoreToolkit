using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CoreToolkit.Editor.Design.Hierarchy
{
    [InitializeOnLoad]
    public static class HierarchyIconDisplay
    {
        private static bool _hierarchyHasFocus;
        
        private static EditorWindow _hierarchyEditorWindow;
        
        static HierarchyIconDisplay()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGui;
            EditorApplication.update += OnEditorUpdate;
        }
        
        private static void OnEditorUpdate()
        {
            if(_hierarchyEditorWindow == null)
            {
                _hierarchyEditorWindow = EditorWindow.GetWindow(Type.GetType(
                    "UnityEditor.SceneHierarchyWindow,UnityEditor"));
            }
            
            _hierarchyHasFocus = EditorWindow.focusedWindow != null && 
                                 EditorWindow.focusedWindow == _hierarchyEditorWindow;
        }
        
        private static void OnHierarchyWindowItemOnGui(int instanceId, Rect selectionRect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            
            if (obj == null) return;
        
            // Skip objects that come from a prefab asset.
            if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj) != null)
            {
                return;
            }
            
            // Get all components on the object.
            Component[] components = obj.GetComponents<Component>();
            
            if (components == null || components.Length == 0) return;
            
            Component component = components.Length > 1 ? components[1] : components[0];
            
            if (component == null) return;
            
            Type type = component.GetType();
            
            if (type == typeof(CanvasRenderer) && components.Length > 2)
            {
                component = components[2]; // display textMeshPro or image instead of canvas renderer
                type = component.GetType();
            }
            
            GUIContent content = EditorGUIUtility.ObjectContent(component, type);
            
            content.text = null;
            
            content.tooltip = type.Name;
            
            if(content.image == null) return;
        
            bool isSelected = Selection.instanceIDs.Contains(instanceId);
            
            bool isHovering = selectionRect.Contains(Event.current.mousePosition);
            
            Color color = UnityEditorBackgroundColor.Get(isSelected, isHovering, _hierarchyHasFocus);
            
            Rect backgroundRect = selectionRect;
            
            backgroundRect.width = 18.5f;
            
            EditorGUI.DrawRect(backgroundRect, color);
            
            EditorGUI.LabelField(selectionRect, content);
        }
    }
}


