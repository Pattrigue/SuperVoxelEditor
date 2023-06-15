using System;
using System.Collections.Generic;
using UnityEngine;

namespace SemagGames.VoxelEditor
{
    public class MeshBuilder 
    {
        private readonly List<Vector3> vertices;
        private readonly List<int> triangles;
        private readonly List<Color32> colors;
    
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

            if (!isBackFace) 
            {
                triangles.Add(this.vertices.Count - 4);
                triangles.Add(this.vertices.Count - 3);
                triangles.Add(this.vertices.Count - 2);
            
                triangles.Add(this.vertices.Count - 4);
                triangles.Add(this.vertices.Count - 2);
                triangles.Add(this.vertices.Count - 1);
            } 
            else 
            {
                triangles.Add(this.vertices.Count - 2);
                triangles.Add(this.vertices.Count - 3);
                triangles.Add(this.vertices.Count - 4);
            
                triangles.Add(this.vertices.Count - 1);
                triangles.Add(this.vertices.Count - 2);
                triangles.Add(this.vertices.Count - 4);
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