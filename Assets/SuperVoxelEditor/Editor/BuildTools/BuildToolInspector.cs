using System;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor.BuildTools
{
    public sealed class BuildToolInspector
    {
        public event Action<BuildTool> BuildToolChanged;
        
        public BuildTool SelectedTool
        {
            get => selectedTool;
            set
            {
                if (selectedTool == value) return;
                
                selectedTool = value;
                BuildToolChanged?.Invoke(selectedTool);
            }
        }

        private readonly BuildToolIcons buildToolIcons;
        private BuildTool selectedTool = BuildTool.Attach;

        public BuildToolInspector()
        {
            buildToolIcons = new BuildToolIcons();
        }
    
        public void Draw()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUIStyle buttonStyle = CreateSquareButtonStyle(32f);
                    
                foreach (BuildTool tool in Enum.GetValues(typeof(BuildTool)))
                {
                    GUIContent buttonContent = new GUIContent(buildToolIcons.GetIcon(tool), tool.ToString());
    
                    if (GUILayout.Toggle(SelectedTool == tool, buttonContent, buttonStyle))
                    {
                        SelectedTool = tool;
                    }
                }
            }
        }
    
        private static GUIStyle CreateSquareButtonStyle(float size)
        {
            return new GUIStyle(GUI.skin.button) { fixedWidth = size, fixedHeight = size };
        }
    }
}