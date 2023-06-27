using System.Collections.Generic;
using SemagGames.SuperVoxelEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class BuildToolIcons
    {
        private readonly Dictionary<BuildToolType, Texture2D> toolIcons;
    
        public BuildToolIcons()
        {
            toolIcons = LoadToolIcons();
        }
    
        private static Dictionary<BuildToolType, Texture2D> LoadToolIcons()
        {
            return new Dictionary<BuildToolType, Texture2D>
            {
                { BuildToolType.Attach, Resources.Load<Texture2D>("AttachIcon") },
                { BuildToolType.Erase, Resources.Load<Texture2D>("EraseIcon") },
                { BuildToolType.Paint, Resources.Load<Texture2D>("PaintIcon") },
                { BuildToolType.Picker, Resources.Load<Texture2D>("PickerIcon") },
            };
        }
    
        public Texture2D GetIcon(BuildToolType buildToolType)
        {
            return toolIcons[buildToolType];
        }
    }
}