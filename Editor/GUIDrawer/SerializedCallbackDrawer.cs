using System;
using System.Linq;
using System.Reflection;
using CoreToolkit.Runtime.Helpers;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using ValueType = CoreToolkit.Runtime.Helpers.ValueType;

namespace CoreToolkit.Editor.GUIDrawer
{
    [CustomPropertyDrawer(typeof(SerializedCallback<>), true)]
    public class SerializedCallbackDrawer : PropertyDrawer 
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            VisualElement root = new ();

            SerializedProperty targetProp = property.FindPropertyRelative("targetObject");
            ObjectField targetField = new ("Target") {
                objectType = typeof(Object),
                bindingPath = targetProp.propertyPath
            };
            root.Add(targetField);

            SerializedProperty methodProp = property.FindPropertyRelative("methodName");
            Button methodField = new () {
                text = string.IsNullOrEmpty(methodProp.stringValue) ? "Select Method" : methodProp.stringValue
            };
            root.Add(methodField);

            methodField.clicked += () => ShowMethodDropdown(targetProp.objectReferenceValue, methodProp, property, methodField, root);

            SerializedProperty parametersProp = property.FindPropertyRelative("parameters");
            VisualElement parametersContainer = new ();
            root.Add(parametersContainer);

            UpdateParameters(parametersProp, parametersContainer);

            property.serializedObject.ApplyModifiedProperties();

            return root;
        }

        void ShowMethodDropdown(Object target, SerializedProperty methodProp, SerializedProperty property, Button methodButton, VisualElement root) {
            if (target == null) return;

            GenericMenu menu = new ();
            Type targetType = target.GetType();

            Type callbackType = fieldInfo.FieldType;
            Type genericType = callbackType.GetGenericArguments()[0];
            if (callbackType.IsGenericType) {
                var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.ReturnType == genericType)
                    .ToArray();

                foreach (MethodInfo method in methods) {
                    menu.AddItem(
                        new GUIContent(method.Name),
                        false,
                        () => {
                            methodProp.stringValue = method.Name;
                            methodButton.text = method.Name;

                            SerializedProperty parametersProp = property.FindPropertyRelative("parameters");
                            var parameters = method.GetParameters();
                            parametersProp.arraySize = parameters.Length;

                            for (int i = 0; i < parameters.Length; i++) {
                                SerializedProperty paramProp = parametersProp.GetArrayElementAtIndex(i);
                                SerializedProperty typeProp = paramProp.FindPropertyRelative("type");
                                typeProp.enumValueIndex = (int) AnyValue.ValueTypeOf(parameters[i].ParameterType);
                            }

                            property.serializedObject.ApplyModifiedProperties();

                            VisualElement parametersContainer = root.Children().Last();
                            parametersContainer.Clear();
                            UpdateParameters(parametersProp, parametersContainer);
                        }
                    );

                    if (!methods.Any()) {
                        menu.AddDisabledItem(new GUIContent("No methods found"));
                    }

                    menu.ShowAsContext();
                }
            }
        }

        void UpdateParameters(SerializedProperty parametersProp, VisualElement container) {
            if (!parametersProp.isArray) return;

            for (int i = 0; i < parametersProp.arraySize; i++) {
                SerializedProperty parameter = parametersProp.GetArrayElementAtIndex(i);
                SerializedProperty typeProp = parameter.FindPropertyRelative("type");
            
                ValueType paramType = (ValueType) typeProp.enumValueIndex;
                VisualElement field;

                switch (paramType) {
                    case ValueType.Int:
                        SerializedProperty intProp = parameter.FindPropertyRelative("intValue");
                        IntegerField intField = new ($"Parameter {i + 1} (Int)");
                        intField.value = intProp.intValue;
                        intField.RegisterValueChangedCallback(
                            evt => {
                                intProp.intValue = evt.newValue;
                                parametersProp.serializedObject.ApplyModifiedProperties();
                            }
                        );
                        field = intField;
                        break;
                
                    case ValueType.Float:
                        SerializedProperty floatProp = parameter.FindPropertyRelative("floatValue");
                        FloatField floatField = new ($"Parameter {i + 1} (Float)");
                        floatField.value = floatProp.floatValue;
                        floatField.RegisterValueChangedCallback(
                            evt => {
                                floatProp.floatValue = evt.newValue;
                                parametersProp.serializedObject.ApplyModifiedProperties();
                            }
                        );
                        field = floatField;
                        break;

                    case ValueType.String:
                        SerializedProperty stringProp = parameter.FindPropertyRelative("stringValue");
                        TextField stringField = new ($"Parameter {i + 1} (String)");
                        stringField.value = stringProp.stringValue;
                        stringField.RegisterValueChangedCallback(
                            evt => {
                                stringProp.stringValue = evt.newValue;
                                parametersProp.serializedObject.ApplyModifiedProperties();
                            }
                        );
                        field = stringField;
                        break;

                    case ValueType.Bool:
                        SerializedProperty boolProp = parameter.FindPropertyRelative("boolValue");
                        Toggle boolField = new ($"Parameter {i + 1} (Bool)");
                        boolField.value = boolProp.boolValue;
                        boolField.RegisterValueChangedCallback(
                            evt => {
                                boolProp.boolValue = evt.newValue;
                                parametersProp.serializedObject.ApplyModifiedProperties();
                            }
                        );
                        field = boolField;
                        break;

                    case ValueType.Vector3:
                        SerializedProperty vector3Prop = parameter.FindPropertyRelative("vector3Value");
                        Vector3Field vector3Field = new ($"Parameter {i + 1} (Vector3)");
                        vector3Field.value = vector3Prop.vector3Value;
                        vector3Field.RegisterValueChangedCallback(
                            evt => {
                                vector3Prop.vector3Value = evt.newValue;
                                parametersProp.serializedObject.ApplyModifiedProperties();
                            }
                        );
                        field = vector3Field;
                        break;

                    default:
                        field = new Label($"Parameter {i + 1}: Unsupported Type");
                        break;
                }
            
                container.Add(field);
            }
        }
    }
}