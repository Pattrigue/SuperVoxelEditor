using System;
using SemagGames.SuperVoxelEditor;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public enum BuildModes { Voxel, Box }
    public enum Shapes { Cube, Sphere }

    public sealed class VoxelVolumeInspector
    {
        public event Action<BuildModes> SelectedBuildModeChanged;
        
        public Shapes SelectedShape { get; private set; } = Shapes.Cube;

        public BuildModes SelectedBuildMode
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
        
        private BuildModes selectedBuildMode = BuildModes.Voxel;

        private bool foldout;
        
        public void DrawInspectorGUI(VoxelVolumeEditor editor, SerializedObject serializedObject)
        {    
            EditorGUILayout.PropertyField(serializedObject.FindProperty("voxelProperty"));

            VoxelVolume volume = (VoxelVolume)editor.target;

            if (volume.VoxelProperty == null)
            {
                EditorGUILayout.HelpBox("You have not assigned a Voxel Property - placing a voxel will default to placing Air voxels!", MessageType.Warning);
            }

            DrawChunkBounds = EditorGUILayout.Toggle("Draw Chunk Bounds", DrawChunkBounds);
            SelectedBuildMode = (BuildModes)EditorGUILayout.EnumPopup("Build Mode", SelectedBuildMode);

            if (selectedBuildMode == BuildModes.Voxel)
            {
                SelectedShape = (Shapes)EditorGUILayout.EnumPopup("Shape", SelectedShape);
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
            editor.BuildTools.DrawInspectorGUI();
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