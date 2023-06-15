using System.Collections.Generic;
using UnityEngine;

namespace SemagGames.VoxelEditor
{
    public sealed class MeshBuilder 
    {
        private readonly List<Vertex> vertices;
        private readonly List<ushort> triangles;

        private int vertexCount;
    
        public MeshBuilder() 
        {
            vertices = new List<Vertex>();
            triangles = new List<ushort>();
        }
    
        public void AddSquareFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, Color32 color, bool isBackFace)
        {
            // Add the 4 vertices, and color for each vertex.
            vertices.Add(new Vertex(a, normal, color));
            vertices.Add(new Vertex(b, normal, color));
            vertices.Add(new Vertex(c, normal, color));
            vertices.Add(new Vertex(d, normal, color));

            int i = vertexCount;
            vertexCount += 4;

            if (!isBackFace) 
            {
                triangles.Add((ushort)i);                
                triangles.Add((ushort)(i + 1));
                triangles.Add((ushort)(i + 2));
                
                triangles.Add((ushort)i);              
                triangles.Add((ushort)(i + 2));
                triangles.Add((ushort)(i + 3));
            } 
            else 
            {
                triangles.Add((ushort)(i + 2));
                triangles.Add((ushort)(i + 1));
                triangles.Add((ushort)i);
                triangles.Add((ushort)(i + 3));
                triangles.Add((ushort)(i + 2));
                triangles.Add((ushort)i);
            }
        }
    
        public MeshData ToMeshData() 
        {
            MeshData data = new MeshData(
                vertices.ToArray(),
                triangles.ToArray()
            );
    
            vertices.Clear();
            triangles.Clear();
            
            return data;
        }
    }
}