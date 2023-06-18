using UnityEngine;

namespace SemagGames.SuperVoxelEditor
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color32 Color;

        public Vertex(Vector3 position, Vector3 normal, Color32 color)
        {
            Position = position;
            Normal = normal;
            Color = color;
        }
    }
}