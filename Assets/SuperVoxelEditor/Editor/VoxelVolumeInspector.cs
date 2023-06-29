using System;
using SemagGames.SuperVoxelEditor;
using SuperVoxelEditor.Editor.BuildModes;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class VoxelVolumeInspector
    {
        public event Action<BuildMode> SelectedBuildModeChanged;
        
        public Shape SelectedShape { get; private set; } = Shape.Cube;

        public BuildMode SelectedBuildMode
        {
            get => selectedBuildMode;
            private set
            {
                if (selectedBuildMode == value) return;
                
                selectedBuildMode = value;
                SelectedBuildModeChanged?.Invoke(selectedBuildMode);
            }
        }

        public bool IsEditingActive { get; private set; } = true;
        public bool DrawChunkBounds { get; private set; } = true;
        
        public int VoxelSize { get; private set; } = 1;
        
        private BuildMode selectedBuildMode = BuildMode.Voxel;

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
            SelectedBuildMode = (BuildMode)EditorGUILayout.EnumPopup("Build Mode", SelectedBuildMode);

            if (selectedBuildMode == BuildMode.Voxel)
            {
                SelectedShape = (Shape)EditorGUILayout.EnumPopup("Shape", SelectedShape);
                VoxelSize = EditorGUILayout.IntSlider("Voxel Size", VoxelSize, 1, 100);
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

            // foldout = EditorGUILayout.Foldout(foldout, "References");
            //
            // if (foldout)
            // {
            //     EditorGUILayout.PropertyField(serializedObject.FindProperty("chunkPrefab"));
            // }

            serializedObject.ApplyModifiedProperties(); // Apply changes after all fields have been drawn
        }
    }
}