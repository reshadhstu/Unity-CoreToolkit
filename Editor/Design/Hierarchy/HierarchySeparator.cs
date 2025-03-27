using UnityEditor;
using UnityEngine;

namespace CoreToolkit.Editor.Design.Hierarchy
{
    [InitializeOnLoad]
    public static class HierarchySeparator
    {
        // Define the gradient colors for the separator background
        private static readonly Color[] GradientColors = 
        {
            new(0f, 0.482f, 0.655f, 1f),   // Medium-dark blue (Cerulean)
            new(0.616f, 0.808f, 0.718f, 1f), // Soft cyan/teal (Pastel Teal)
            new(0.980f, 0.855f, 0.867f, 1f)  // Pale pinkish-white (Pale Pink)
        };

        // Register the hierarchy item GUI handler on load
        static HierarchySeparator()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyItemGUI;
        }

        // Handle drawing of each hierarchy item
        private static void HandleHierarchyItemGUI(int instanceID, Rect selectionRect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (obj == null) return;

            // Check if the object name starts with "--" to identify it as a separator
            if (obj.name.StartsWith("--", System.StringComparison.OrdinalIgnoreCase))
            {
                // Remove dashes from the name for display
                string separatorText = obj.name.Trim('-');
                
                // Use the full selection rect for drawing
                Rect separatorRect = new Rect(selectionRect);

                // Draw the gradient background
                DrawGradientBackground(separatorRect);

                // Define the style for the separator text
                GUIStyle separatorStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.white }
                };

                // Draw the separator text in uppercase
                EditorGUI.LabelField(separatorRect, separatorText.ToUpper(), separatorStyle);
            }
        }

        // Draw the gradient background for the separator
        private static void DrawGradientBackground(Rect rect)
        {
            // Create a vertical gradient texture (1x100 pixels)
            Texture2D gradientTexture = new Texture2D(1, 100);
            for (int i = 0; i < 100; i++)
            {
                float t = i / 99f; // Interpolation value from 0 to 1
                // Lerp between the first two colors, then towards the third
                Color color = Color.Lerp(Color.Lerp(GradientColors[0], GradientColors[1], t), GradientColors[2], t);
                gradientTexture.SetPixel(0, i, color);
            }
            gradientTexture.Apply();

            // Draw the texture stretched across the rect
            GUI.DrawTexture(rect, gradientTexture, ScaleMode.StretchToFill);
        }

        // Menu item to create a new separator with shortcut Shift + S
        [MenuItem("GameObject/Create Separator _#S", false, 0)]
        private static void CreateSeparator()
        {
            // Create a new GameObject named "--Separator"
            GameObject separator = new GameObject("--Separator");

            // If an object is selected, position the separator relative to it
            if (Selection.activeGameObject != null)
            {
                separator.transform.SetParent(Selection.activeGameObject.transform.parent);
                separator.transform.SetSiblingIndex(Selection.activeGameObject.transform.GetSiblingIndex() + 1);
            }

            // Select the new separator and focus the hierarchy window
            Selection.activeGameObject = separator;
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
        }
    }
}