using System;
using UnityEngine;

[Serializable]
public class ColorPicker
{
    [SerializeField] private Texture2D colorPaletteTexture;
    [SerializeField] private Color selectedColor;

    public Color SelectedColor => selectedColor;
}