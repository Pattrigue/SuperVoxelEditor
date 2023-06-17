﻿using UnityEngine.Rendering;
using UnityEngine;

namespace SemagGames.VoxelEditor
{
    [ExecuteAlways]
    public sealed class ChunkMesh : MonoBehaviour
    {
        [SerializeField] private BorderVisibilityFlags borderVisibilityFlags = BorderVisibilityFlags.Everything;
        
        private const MeshUpdateFlags MeshUpdateFlags = UnityEngine.Rendering.MeshUpdateFlags.DontRecalculateBounds
                                                        | UnityEngine.Rendering.MeshUpdateFlags.DontValidateIndices
                                                        | UnityEngine.Rendering.MeshUpdateFlags.DontNotifyMeshUsers
                                                        | UnityEngine.Rendering.MeshUpdateFlags.DontResetBoneBounds;

        private static readonly VertexAttributeDescriptor[] VertexAttributeDescriptors =
        {
            new(VertexAttribute.Position),
            new(VertexAttribute.Normal),
            new(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4)
        };
        
        private Chunk chunk;
        private Mesh mesh;
        private MeshCollider meshCollider;
        private MeshFilter meshFilter;
        private ChunkMeshBuilder meshBuilder;

        private void Awake()
        {
            chunk = GetComponent<Chunk>();
            meshFilter = GetComponent<MeshFilter>();
            meshCollider = GetComponent<MeshCollider>();
        }

        public void Build()
        {
            Debug.Log($"Building chunk mesh at {chunk.ChunkPosition}", gameObject);

            ResetMesh();

            MeshData meshData = meshBuilder.GenerateMeshData(borderVisibilityFlags);
            
            mesh.SetVertexBufferParams(meshData.Vertices.Length, VertexAttributeDescriptors);
            mesh.SetVertexBufferData(meshData.Vertices.AsArray(), 0, 0, meshData.Vertices.Length, 0, MeshUpdateFlags);
            
            mesh.SetIndexBufferParams(meshData.Triangles.Length, IndexFormat.UInt16);
            mesh.SetIndexBufferData(meshData.Triangles.AsArray(), 0, 0, meshData.Triangles.Length, MeshUpdateFlags);
            
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, meshData.Triangles.Length), MeshUpdateFlags);
            mesh.RecalculateBounds();
            
            if (mesh.vertices.Length == 0)
            {
                DestroyImmediate(gameObject);
                return;
            }

            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
            meshBuilder.Dispose();
        }

        private void ResetMesh()
        {
            meshCollider.sharedMesh = null;
            meshFilter.sharedMesh = null;
            
            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.MarkDynamic();
            }
            else
            {
                mesh.Clear(false);
            }
                
            meshBuilder = new ChunkMeshBuilder(chunk);
        }
    }
}