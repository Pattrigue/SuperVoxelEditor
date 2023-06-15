using UnityEditor;
using UnityEngine;

namespace SemagGames.VoxelEditor.Editor
{
    [CustomEditor(typeof(VoxelTerrainTester))]
    public sealed class VoxelTerrainTesterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Rebuild"))
            {
                ((VoxelTerrainTester)target).Rebuild();
            }
        }
    }
}