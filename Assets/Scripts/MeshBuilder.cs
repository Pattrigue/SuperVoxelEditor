using System;
using System.Collections.Generic;
using UnityEngine;

namespace SemagGames.VoxelEditor
{
    public sealed class MeshBuilder 
    {
        private readonly List<Vector3> vertices;
        private readonly List<int> triangles;
        private readonly List<Color32> colors;

        private int vertexCount;
    
        public MeshBuilder() 
        {
            vertices = new List<Vector3>();
            triangles = new List<int>();
            colors = new List<Color32>();
        }
    
        public void AddSquareFace(Vector3[] vertices, Color32 color, bool isBackFace)
        {
            if (vertices.Length != 4) 
            {
                throw new ArgumentException("A square face requires 4 vertices");
            }
    
            // Add the 4 vertices, and color for each vertex.
            for (int i = 0; i < vertices.Length; i++) 
            {
                this.vertices.Add(vertices[i]);
                colors.Add(color);
            }

            int triangleIndex = vertexCount;
            
            vertexCount += 4;

            if (!isBackFace) 
            {
                triangles.Add(triangleIndex);                
                triangles.Add(triangleIndex + 1);
                triangles.Add(triangleIndex + 2);
            
                triangles.Add(triangleIndex);               
                triangles.Add(triangleIndex + 2);
                triangles.Add(triangleIndex + 3);
            } 
            else 
            {
                triangles.Add(triangleIndex + 2);
                triangles.Add(triangleIndex + 1);
                triangles.Add(triangleIndex);            
                triangles.Add(triangleIndex + 3);
                triangles.Add(triangleIndex + 2);
                triangles.Add(triangleIndex);            
            }
        }
    
        public MeshData ToMeshData() 
        {
            MeshData data = new MeshData(
                vertices.ToArray(),
                triangles.ToArray(),
                colors.ToArray()
            );
    
            vertices.Clear();
            triangles.Clear();
            colors.Clear();
    
            return data;
        }
    }
}