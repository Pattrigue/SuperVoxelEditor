using UnityEditor;
using UnityEngine;

namespace SemagGames.VoxelEditor
{
    [CustomEditor(typeof(VoxelAssetDatabase))]
    public sealed class VoxelAssetDatabaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Get Voxel Assets In Project"))
            {
                ((VoxelAssetDatabase)target).GetVoxelAssetsInProject();
            }
        }
    }
}