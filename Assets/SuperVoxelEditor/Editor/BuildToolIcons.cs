using System.Collections.Generic;
using SemagGames.SuperVoxelEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class BuildToolIcons
    {
        private readonly Dictionary<BuildTool, Texture2D> toolIcons;
    
        public BuildToolIcons()
        {
            toolIcons = LoadToolIcons();
        }
    
        private static Dictionary<BuildTool, Texture2D> LoadToolIcons()
        {
            return new Dictionary<BuildTool, Texture2D>
            {
                { BuildTool.Attach, Resources.Load<Texture2D>("AttachIcon") },
                { BuildTool.Erase, Resources.Load<Texture2D>("EraseIcon") },
                { BuildTool.Paint, Resources.Load<Texture2D>("PaintIcon") },
                { BuildTool.Picker, Resources.Load<Texture2D>("PickerIcon") },
            };
        }
    
        public Texture2D GetIcon(BuildTool buildTool)
        {
            return toolIcons[buildTool];
        }
    }
}