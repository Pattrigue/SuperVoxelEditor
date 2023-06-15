using UnityEngine;
    
namespace SemagGames.VoxelEditor
{
    public sealed class MeshData
    {
        public Vector3[] Vertices { get; }
        public int[] Triangles { get; }
        public Color32[] Colors { get; }

        public MeshData(Vector3[] vertices, int[] triangles, Color32[] colors)
        {
            Vertices = vertices;
            Triangles = triangles;
            Colors = colors;
        }
    }
}