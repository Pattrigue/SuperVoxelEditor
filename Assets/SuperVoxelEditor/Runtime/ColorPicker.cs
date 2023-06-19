using System;
using UnityEngine;

namespace SemagGames.SuperVoxelEditor
{
    [Serializable]
    public class ColorPicker
    {
        [SerializeField] private Texture2D colorPaletteTexture;
        [SerializeField] private Color32[] colors;
        [SerializeField] private Color32 selectedColor;
        [SerializeField] private uint selectedColorIndex;

        public Color32 SelectedColor => selectedColor;
        
        public uint SelectedColorIndex => selectedColorIndex;

        public void SetColor(uint index)
        {
            selectedColorIndex = index;
            selectedColor = colors[index];
        }

        public Color32 GetColorByIndex(uint index)
        {
            return colors[index];
        }
    }
}