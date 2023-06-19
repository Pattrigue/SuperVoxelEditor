using System;
using System.Collections.Generic;
using SemagGames.SuperVoxelEditor;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class VoxelVolumeInspectorDrawer
    {
        public BuildTool SelectedTool { get; private set; } = BuildTool.Attach;
        
        public bool IsEditingActive { get; private set; } = true;
        public bool DrawChunkBounds { get; private set; } = true;
        
        private bool foldout;

        private readonly Dictionary<BuildTool, Texture2D> toolIcons;

        public VoxelVolumeInspectorDrawer()
        {   
            toolIcons = new Dictionary<BuildTool, Texture2D>
            {
                { BuildTool.Attach, Resources.Load<Texture2D>("AttachIcon") },
                { BuildTool.Erase, Resources.Load<Texture2D>("EraseIcon") },
                { BuildTool.Paint, Resources.Load<Texture2D>("PaintIcon") },
                { BuildTool.Picker, Resources.Load<Texture2D>("PickerIcon") },
            };
        }

        public void DrawInspectorGUI(VoxelVolumeEditor editor, SerializedObject serializedObject)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("voxelProperty"));

            VoxelVolume volume = (VoxelVolume)editor.target;
    
            if (volume.VoxelProperty == null)
            {
                EditorGUILayout.HelpBox("You have not assigned a Voxel Property - placing a voxel will default to placing Air voxels!", MessageType.Warning);
            }

            DrawChunkBounds = EditorGUILayout.Toggle("Draw Chunk Bounds", DrawChunkBounds);

            // Create a GUIStyle for headers
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14, 
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(0, 0, 0, 0) 
            };
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Build Tools", headerStyle);
            DrawToolButtons();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Color Palette", headerStyle);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("colorPicker"));
            GUILayout.Label("", GUILayout.Height(-EditorGUIUtility.singleLineHeight * 0.75f)); // hack to shrink the box
            EditorGUILayout.EndVertical();

            if (!IsEditingActive && GUILayout.Button("Edit"))
            {
                IsEditingActive = true;
            }
            else if (IsEditingActive && GUILayout.Button("Stop Editing"))
            {
                IsEditingActive = false;
            }

            if (GUILayout.Button("Clear Volume") && EditorUtility.DisplayDialog("Clear Volume", "Are you sure you want to clear the volume?", "Yes", "No"))
            {
                volume.Clear();
            }

            foldout = EditorGUILayout.Foldout(foldout, "References");
    
            if (foldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("chunkPrefab"));
            }

            serializedObject.ApplyModifiedProperties(); // Apply changes after all fields have been drawn
        }

        private void DrawToolButtons()
        {
            GUILayout.BeginHorizontal();
        
            // Store the original button height to restore after creating the square buttons
            float originalButtonHeight = GUI.skin.button.fixedHeight;
            
            // Set the button height to match the current width (makes it square) 
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 48f,
                fixedHeight = 48f 
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
    }
}