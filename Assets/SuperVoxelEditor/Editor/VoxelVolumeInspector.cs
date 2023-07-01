using System;
using SemagGames.SuperVoxelEditor;
using SuperVoxelEditor.Editor.BuildModes;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class VoxelVolumeInspector
    {
        public bool IsEditingActive { get; private set; } = true;
        public bool DrawChunkBounds { get; private set; } 
        
        private bool foldout;
        
        public void DrawInspectorGUI(VoxelVolumeEditor editor, SerializedObject serializedObject)
        {    
            EditorGUILayout.PropertyField(serializedObject.FindProperty("voxelProperty"));

            VoxelVolume volume = (VoxelVolume)editor.target;

            if (volume.VoxelProperty == null)
            {
                EditorGUILayout.HelpBox("You have not assigned a Voxel Property - placing a voxel will default to placing Air voxels!", MessageType.Warning);
            }

            volume.VoxelColor = EditorGUILayout.ColorField("Voxel Color", volume.VoxelColor);

            DrawChunkBounds = EditorGUILayout.Toggle("Draw Chunk Bounds", DrawChunkBounds);
            
            BuildMode selectedBuildMode = (BuildMode)EditorGUILayout.EnumPopup("Build Mode", editor.ActiveBuildMode.BuildMode);
            
            if (selectedBuildMode != editor.ActiveBuildMode.BuildMode)
            {
                editor.SwitchBuildMode(selectedBuildMode);
            }

            if (editor.ActiveBuildMode is VoxelBuildMode voxelBuildMode)
            {
                voxelBuildMode.SelectedShape = (Shape)EditorGUILayout.EnumPopup("Shape", voxelBuildMode.SelectedShape);
                voxelBuildMode.VoxelSize = EditorGUILayout.IntSlider("Voxel Size", voxelBuildMode.VoxelSize, 1, 100);
            }
            else if (editor.ActiveBuildMode is FaceBuildMode faceBuildMode)
            {
                faceBuildMode.MaxExploreLimit = EditorGUILayout.IntSlider("Max Explore Limit", faceBuildMode.MaxExploreLimit, 1, 1000);
            }

            // Create a GUIStyle for headers
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(0, 0, 0, 0)
            };

            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Build Tools", headerStyle);
            editor.BuildTools.Inspector.Draw();
            EditorGUILayout.EndVertical();

            if (!IsEditingActive && GUILayout.Button("Edit"))
            {
                IsEditingActive = true;
            }
            else if (IsEditingActive && GUILayout.Button("Stop Editing"))
            {
                IsEditingActive = false;
            }

            if (GUILayout.Button("Undo"))
            {
                volume.Undo();
            }
            else if (GUILayout.Button("Redo"))
            {
                volume.Redo();
            }

            if (GUILayout.Button("Clear Volume") && EditorUtility.DisplayDialog("Clear Volume", "Are you sure you want to clear the volume?", "Yes", "No"))
            {
                volume.Clear();
            }

            serializedObject.ApplyModifiedProperties(); // Apply changes after all fields have been drawn
        }
    }
}