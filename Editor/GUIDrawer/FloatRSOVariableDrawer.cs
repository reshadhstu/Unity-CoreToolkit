using CoreToolkit.Runtime.SO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CoreToolkit.Editor.GUIDrawer
{
    [CustomPropertyDrawer(typeof(FloatVariable))]
    public class FloatRSOVariableDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var objectField = new ObjectField(property.displayName)
            {
                objectType = typeof(FloatVariable)
            };
            
            objectField.BindProperty(property);
            
            var valueLabel = new Label
            {
                style =
                {
                    paddingLeft = 20
                }
            };
            
            container.Add(objectField);
            container.Add(valueLabel);

            objectField.RegisterValueChangedCallback(
                evt =>
                {
                    var variable = evt.newValue as FloatVariable;

                    if (variable != null)
                    {
                        valueLabel.text = $"Current Value: {variable.Value}";
                        variable.OnValueChanged += newValue => valueLabel.text = $"Current Value: {newValue}";
                    }
                    else
                    {
                        valueLabel.text = string.Empty;
                    }
                });
            
            var currentVariable = property.objectReferenceValue as FloatVariable;

            if (currentVariable != null)
            {
                valueLabel.text = $"Current Value: {currentVariable.Value}";
                currentVariable.OnValueChanged += newValue => valueLabel.text = $"Current Value: {newValue}";
            }

            return container;
        }
    }
}