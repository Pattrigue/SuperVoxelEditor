using System;
using System.Collections.Generic;
using SemagGames.SuperVoxelEditor;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class BuildTools
    {
        public BuildTool SelectedTool { get; private set; } = BuildTool.Attach;

        private readonly Dictionary<BuildTool, Texture2D> toolIcons;
        
        public BuildTools()
        {
            toolIcons = new Dictionary<BuildTool, Texture2D>
            {
                { BuildTool.Attach, Resources.Load<Texture2D>("AttachIcon") },
                { BuildTool.Erase, Resources.Load<Texture2D>("EraseIcon") },
                { BuildTool.Paint, Resources.Load<Texture2D>("PaintIcon") },
                { BuildTool.Picker, Resources.Load<Texture2D>("PickerIcon") },
            };
        }
        
        public void DrawInspectorGUI()
        {
            GUILayout.BeginHorizontal();
        
            // Store the original button height to restore after creating the square buttons
            float originalButtonHeight = GUI.skin.button.fixedHeight;
            
            // Set the button height to match the current width (makes it square) 
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 32f,
                fixedHeight = 32f
            };
           
            foreach (BuildTool tool in Enum.GetValues(typeof(BuildTool)))
            {
                GUIContent buttonContent = new GUIContent(toolIcons[tool], tool.ToString());
                
                if (GUILayout.Toggle(SelectedTool == tool, buttonContent, buttonStyle))
                {
                    SelectedTool = tool;
                }
            }
        
            // Restore the original button height
            GUI.skin.button.fixedHeight = originalButtonHeight;
            
            GUILayout.EndHorizontal();
        }
        
        public void HandleKeyPressEvents(Event e)
        {
            if (!e.shift)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Q:
                        CycleTool(-1);
                        break;
                    case KeyCode.E:
                        CycleTool(1);
                        break;
                }
            }
            else
            {
                // Shift + alpha key: switch to a specific tool
                switch (e.keyCode)
                {
                    case KeyCode.Alpha1:
                        SelectedTool = BuildTool.Attach;
                        break;
                    case KeyCode.Alpha2:
                        SelectedTool = BuildTool.Erase;
                        break;
                    case KeyCode.Alpha3:
                        SelectedTool = BuildTool.Paint;
                        break;
                    case KeyCode.Alpha4:
                        SelectedTool = BuildTool.Picker;
                        break;
                }
            }
        }

        public void PickVoxelAtPosition(VoxelVolume volume, Vector3 voxelPosition)
        {
            if (!volume.TryGetVoxel(voxelPosition, out Voxel voxel)) return;
            
            string[] voxelProperties = AssetDatabase.FindAssets("t:VoxelProperty");
                                
            foreach (string voxelProperty in voxelProperties)
            {
                VoxelProperty voxelPropertyObject = AssetDatabase.LoadAssetAtPath<VoxelProperty>(AssetDatabase.GUIDToAssetPath(voxelProperty));
                                    
                if (voxelPropertyObject.ID == voxel.propertyId)
                {
                    volume.VoxelProperty = voxelPropertyObject;
                    break;
                }
            }
                                
            volume.ColorPicker.SetColor(voxel.colorId);
            SelectedTool = BuildTool.Attach;
        }
        
        private void CycleTool(int direction)
        {
            int toolCount = Enum.GetValues(typeof(BuildTool)).Length;
            int toolIndex = (int)SelectedTool + direction;
            toolIndex = (toolIndex + toolCount) % toolCount;
            SelectedTool = (BuildTool)toolIndex;
        }
        
    }
}