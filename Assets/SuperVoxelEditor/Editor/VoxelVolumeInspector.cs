using SuperVoxelEditor.Editor.BuildModes;
using SuperVoxelEditor.Editor.BuildTools;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class VoxelVolumeInspector
    {
        public bool IsEditingActive { get; private set; } = true;
        public bool DrawChunkBounds { get; private set; } 
        
        private bool foldout;

        private VoxelVolumeEditor editor;
        
        public void DrawInspectorGUI(VoxelVolumeEditor editor, SerializedObject serializedObject)
        {
            this.editor = editor;

            DrawVoxelAssetField(serializedObject);
            DrawChunkBoundsToggle();
            DrawBuildModeSelector();
            DrawBuildModeSpecificSettings();
            DrawBuildTools();
            DrawEditingButtons();
            DrawUndoRedoButtons();
            DrawClearVolumeButton();
            
            serializedObject.ApplyModifiedProperties(); 
        }

        private void DrawVoxelAssetField(SerializedObject serializedObject)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("voxelAsset"));

            if (editor.Volume.VoxelAsset == null)
            {
                EditorGUILayout.HelpBox("You have not assigned a Voxel Asset - placing a voxel will default to placing Air voxels!", MessageType.Warning);
            }
        }

        private void DrawChunkBoundsToggle()
        {
            DrawChunkBounds = EditorGUILayout.Toggle("Draw Chunk Bounds", DrawChunkBounds);
        }

        private void DrawBuildModeSelector()
        {
            BuildMode selectedBuildMode = (BuildMode)EditorGUILayout.EnumPopup("Build Mode", editor.ActiveBuildMode.BuildMode);
            
            if (selectedBuildMode != editor.ActiveBuildMode.BuildMode)
            {
                editor.SwitchBuildMode(selectedBuildMode);
            }
        }

        private void DrawBuildModeSpecificSettings()
        {
            if (editor.ActiveBuildMode is VoxelBuildMode voxelBuildMode)
            {
                voxelBuildMode.SelectedShape = (Shape)EditorGUILayout.EnumPopup("Shape", voxelBuildMode.SelectedShape);
                voxelBuildMode.VoxelSize = EditorGUILayout.IntSlider("Voxel Size", voxelBuildMode.VoxelSize, 1, 100);
            }
            else if (editor.ActiveBuildMode is FaceBuildMode faceBuildMode)
            {
                faceBuildMode.MaxExploreLimit = EditorGUILayout.IntSlider("Max Explore Limit", faceBuildMode.MaxExploreLimit, 1, 10000);

                if (editor.BuildTools.SelectedTool == BuildTool.Attach)
                {
                    faceBuildMode.ExtrudeHeight = EditorGUILayout.IntSlider("Extrude Height", faceBuildMode.ExtrudeHeight, 1, 16);
                }
            }
        }

        private void DrawBuildTools()
        {
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
        }

        private void DrawEditingButtons()
        {
            if (!IsEditingActive && GUILayout.Button("Edit"))
            {
                IsEditingActive = true;
            }
            else if (IsEditingActive && GUILayout.Button("Stop Editing"))
            {
                IsEditingActive = false;
            }
        }

        private void DrawUndoRedoButtons()
        {
            if (GUILayout.Button("Undo"))
            {
                editor.Volume.Undo();
            }
            else if (GUILayout.Button("Redo"))
            {
                editor.Volume.Redo();
            }
        }

        private void DrawClearVolumeButton()
        {
            if (GUILayout.Button("Clear Volume") && EditorUtility.DisplayDialog("Clear Volume", "Are you sure you want to clear the volume?", "Yes", "No"))
            {
                editor.Volume.Clear();
            }
        }
    }
}