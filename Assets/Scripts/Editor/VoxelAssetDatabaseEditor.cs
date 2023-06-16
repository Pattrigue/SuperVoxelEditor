using UnityEditor;
using UnityEngine;

namespace SemagGames.VoxelEditor.Editor
{
    [CustomEditor(typeof(VoxelProperties))]
    public sealed class VoxelAssetDatabaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Get Voxel Assets In Project"))
            {
                ((VoxelProperties)target).GetVoxelAssetsInProject();
            }
        }
    }
}