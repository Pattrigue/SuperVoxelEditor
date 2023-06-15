using System;
using UnityEngine;

namespace SemagGames.VoxelEditor.ColorPicking
{
    [Serializable]
    public class ColorPicker
    {
        [SerializeField] private Texture2D colorPaletteTexture;
        [SerializeField] private Color selectedColor;

        public Color SelectedColor => selectedColor;
    }
}