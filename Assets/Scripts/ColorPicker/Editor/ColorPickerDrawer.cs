using UnityEditor;
using UnityEngine;

namespace SemagGames.VoxelEditor.ColorPicker.Editor
{
    [CustomPropertyDrawer(typeof(ColorPicker))]
    public sealed class ColorPickerDrawer : PropertyDrawer
    {
        private const string ColorPaletteTexture = "colorPaletteTexture";
        private const string SelectedColor = "selectedColor";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty colorPaletteTextureProp = property.FindPropertyRelative(ColorPaletteTexture);
            SerializedProperty selectedColorProp = property.FindPropertyRelative(SelectedColor);

            // Draw the colorPaletteTexture field
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, colorPaletteTextureProp);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Draw the "Selected Color" label
            EditorGUI.LabelField(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), "Selected Color");

            // Draw the selected color
            float colorBoxWidth = position.width - EditorGUIUtility.labelWidth;
            Rect selectedColorRect = new(position.x + EditorGUIUtility.labelWidth, position.y, colorBoxWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.DrawRect(selectedColorRect, selectedColorProp.colorValue);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (colorPaletteTextureProp.objectReferenceValue != null)
            {
                // Get all colors from the texture
                Texture2D colorPaletteTexture = colorPaletteTextureProp.objectReferenceValue as Texture2D;
                Color[] colors = colorPaletteTexture.GetPixels();

                // Calculate how many colors to display per row
                int colorsPerRow = Mathf.RoundToInt(Mathf.Sqrt(colors.Length));
                float cellWidth = position.width / colorsPerRow;
                float cellSize = cellWidth - 1;

                // Create a transparent GUI style
                GUIStyle emptyStyle = new();

                // Add vertical spacing to the position
                position.y += EditorGUIUtility.standardVerticalSpacing;

                for (int i = 0; i < colors.Length; i++)
                {
                    Rect colorRect = new(position.x + i % colorsPerRow * cellWidth, position.y + i / colorsPerRow * cellWidth, cellSize, cellSize);

                    // If this color is the selected color, draw a white border around it
                    if (colors[i] == selectedColorProp.colorValue) EditorGUI.DrawRect(new Rect(colorRect.x - 1, colorRect.y - 1, cellSize + 2, cellSize + 2), Color.white);

                    EditorGUI.DrawRect(colorRect, colors[i]);

                    // Overlay the button on the colored rectangle
                    if (GUI.Button(colorRect, GUIContent.none, emptyStyle))
                        // Set the selected color when the button is clicked
                        selectedColorProp.colorValue = colors[i];
                }

                position.y += Mathf.CeilToInt((float)colors.Length / colorsPerRow) * cellWidth;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.FindPropertyRelative(ColorPaletteTexture).objectReferenceValue != null)
            {
                Texture2D colorPaletteTexture = property.FindPropertyRelative(ColorPaletteTexture).objectReferenceValue as Texture2D;
                int colorsPerRow = Mathf.RoundToInt(Mathf.Sqrt(colorPaletteTexture.width * colorPaletteTexture.height));
                float cellWidth = EditorGUIUtility.currentViewWidth / colorsPerRow;

                int numberOfRows = Mathf.CeilToInt((float)colorPaletteTexture.width * colorPaletteTexture.height / colorsPerRow);
                return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 3 + numberOfRows * cellWidth;
            }

            return 2 * EditorGUIUtility.singleLineHeight + 2 * EditorGUIUtility.standardVerticalSpacing;
        }
    }
}