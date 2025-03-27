using System;
using System.Collections.Generic;
using CoreToolkit.Editor.Util;
using CoreToolkit.Runtime.Attributes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoreToolkit.Editor.GUIDrawer
{
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    public class RequireInterfaceDrawer : PropertyDrawer 
    {
        RequireInterfaceAttribute RequireInterfaceAttribute => (RequireInterfaceAttribute)attribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Type requiredInterfaceType = RequireInterfaceAttribute.InterfaceType;
            EditorGUI.BeginProperty(position, label, property);

            if (property.isArray && property.propertyType == SerializedPropertyType.Generic) {
                DrawArrayField(position, property, label, requiredInterfaceType);
            } else {
                DrawInterfaceObjectField(position, property, label, requiredInterfaceType);
            }
        
            EditorGUI.EndProperty();
            var args = new InterfaceArgs(GetTypeOrElementType(fieldInfo.FieldType), requiredInterfaceType);
            InterfaceReferenceUtil.OnGUI(position, property, label, args);
        }

        void DrawArrayField(Rect position, SerializedProperty property, GUIContent label, Type interfaceType) {
            property.arraySize = EditorGUI.IntField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                label.text + " Size", property.arraySize);
        
            float yOffset = EditorGUIUtility.singleLineHeight;
            for (int i = 0; i < property.arraySize; i++) {
                var element = property.GetArrayElementAtIndex(i);
                var elementRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                DrawInterfaceObjectField(elementRect, element, new GUIContent($"Element {i}"), interfaceType);
                yOffset += EditorGUIUtility.singleLineHeight;
            }
        }

        void DrawInterfaceObjectField(Rect position, SerializedProperty property, GUIContent label, Type interfaceType) {
            var oldReference = property.objectReferenceValue;
            Type baseType = GetAssignableBaseType(fieldInfo.FieldType, interfaceType);
            var newReference = EditorGUI.ObjectField(position, label, oldReference, baseType, true);

            if (newReference != null && newReference != oldReference) {
                ValidateAndAssignObject(property, newReference, interfaceType);
            } else if (newReference == null) {
                property.objectReferenceValue = null;
            }
        }

        Type GetAssignableBaseType(Type fieldType, Type interfaceType) {
            Type elementType = fieldType.IsArray ? fieldType.GetElementType() :
                fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>) 
                    ? fieldType.GetGenericArguments()[0] 
                    : fieldType;
        
            if (interfaceType.IsAssignableFrom(elementType)) return elementType;

            if (typeof(ScriptableObject).IsAssignableFrom(elementType)) return typeof(ScriptableObject);
            if (typeof(MonoBehaviour).IsAssignableFrom(elementType)) return typeof(MonoBehaviour);

            return typeof(Object);
        }

        void ValidateAndAssignObject(SerializedProperty property, Object newReference, Type interfaceType) {
            if (newReference is GameObject gameObject) {
                var component = gameObject.GetComponent(interfaceType);
                if (component != null) {
                    property.objectReferenceValue = component;
                    return;
                }
            } else if (interfaceType.IsAssignableFrom(newReference.GetType())) {
                property.objectReferenceValue = newReference;
                return;
            }
        
            Debug.LogWarning($"The assigned object does not implement '{interfaceType.Name}'.");
            property.objectReferenceValue = null;
        }

        Type GetTypeOrElementType(Type type) {
            if (type.IsArray) return type.GetElementType();
            if (type.IsGenericType) return type.GetGenericArguments()[0];
            return type;
        }
    }
}