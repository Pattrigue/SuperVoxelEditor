using SemagGames.SuperVoxelEditor;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class VoxelVolumeInspectorDrawer
    {
        public bool IsEditingActive { get; private set; } = true;

        public bool drawChunkBounds = true;
        private bool foldout;

        public void DrawInspectorGUI(VoxelVolumeEditor editor, SerializedObject serializedObject)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("voxelProperty"));

            VoxelVolume volume = (VoxelVolume)editor.target;
            if (volume.VoxelProperty == null)
            {
                EditorGUILayout.HelpBox("You have not assigned a Voxel Property - placing a voxel will default to placing Air voxels!", MessageType.Warning);
            }

            drawChunkBounds = EditorGUILayout.Toggle("Draw Chunk Bounds", drawChunkBounds);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("colorPicker"));

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

    }
}