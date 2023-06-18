using SemagGames.SuperVoxelEditor;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    [CustomEditor(typeof(Chunk))]
    public sealed class ChunkEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            if (GUILayout.Button("Rebuild"))
            {
                ((Chunk)target).Rebuild();
            }
        }
    }
}