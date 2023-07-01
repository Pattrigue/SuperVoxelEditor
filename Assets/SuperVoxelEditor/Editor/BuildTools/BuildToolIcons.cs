using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperVoxelEditor.Editor.BuildTools
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
            return Enum.GetNames(typeof(BuildTool))
                .ToDictionary(Enum.Parse<BuildTool>, buildToolName => Resources.Load<Texture2D>($"{buildToolName}Icon"));
        }
    
        public Texture2D GetIcon(BuildTool buildTool)
        {
            return toolIcons[buildTool];
        }
    }
}