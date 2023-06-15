using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace SemagGames.VoxelEditor
{
    using System.Collections.Generic;
    using UnityEngine;

    [ExecuteAlways]
    public sealed class ChunkMesh : MonoBehaviour
    {
        private const MeshUpdateFlags MeshUpdateFlags = UnityEngine.Rendering.MeshUpdateFlags.DontRecalculateBounds
                                                        | UnityEngine.Rendering.MeshUpdateFlags.DontValidateIndices
                                                        | UnityEngine.Rendering.MeshUpdateFlags.DontNotifyMeshUsers
                                                        | UnityEngine.Rendering.MeshUpdateFlags.DontResetBoneBounds;

        private static readonly Vector3[] VertexPositions =
        {
            new(-0.5f, 0.5f, -0.5f),
            new(-0.5f, 0.5f, 0.5f),
            new(0.5f, 0.5f, 0.5f),
            new(0.5f, 0.5f, -0.5f),
            new(-0.5f, -0.5f, -0.5f),
            new(-0.5f, -0.5f, 0.5f),
            new(0.5f, -0.5f, 0.5f),
            new(0.5f, -0.5f, -0.5f)
        };

        public static readonly Vector3Int Dimensions = new Vector3Int(16, 16, 16);
        
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

        private MeshData meshData;

        private void Awake()
        {
            chunk = GetComponent<Chunk>();
            meshFilter = GetComponent<MeshFilter>();
            meshCollider = GetComponent<MeshCollider>();
        }

        private void OnDestroy()
        {
            meshData.Dispose();
        }

        public void Build(IReadOnlyList<Voxel> voxels)
        {
            Debug.Log($"Building chunk mesh at {chunk.ChunkPosition}");

            ResetMesh();
            GenerateMeshData();
            
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

            meshData.Dispose();
        }

        private void ResetMesh()
        {
            meshData.Dispose(); // ensure to dispose old one to avoid memory leak
            meshData = MeshData.Allocate();
            
            meshCollider.sharedMesh = null;
            meshFilter.sharedMesh = null;
            vertexCount = 0;

            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.MarkDynamic();
            }
            else
            {
                mesh.Clear();
            }
        }

        private int vertexCount;
        
        public void Allocate()
        {
            meshData = MeshData.Allocate();
        }
                
        public void Dispose() 
        {
            vertexCount = 0;
            meshData.Dispose();
        }

        private void GenerateMeshData()
        {
            bool[,] merged;

            Vector3Int startPos, currPos, quadSize, m, n, offsetPos;
            Vector3[] vertices;

            Voxel startVoxel;
            int direction, workAxis1, workAxis2;

            // Iterate over each face of the voxels.
            for (int face = 0; face < 6; face++) 
            {
                bool isBackFace = face % 2 == 1;
                direction = face % 3;
                workAxis1 = (direction + 1) % 3;
                workAxis2 = (direction + 2) % 3;

                startPos = new Vector3Int();
                currPos = new Vector3Int();

                // Iterate over the chunk layer by layer.
                for (startPos[direction] = 0; startPos[direction] < Dimensions[direction]; startPos[direction]++) 
                {
                    merged = new bool[Dimensions[workAxis1], Dimensions[workAxis2]];

                    // Build the slices of the mesh.
                    for (startPos[workAxis1] = 0; startPos[workAxis1] < Dimensions[workAxis1]; startPos[workAxis1]++) 
                    {
                        for (startPos[workAxis2] = 0; startPos[workAxis2] < Dimensions[workAxis2]; startPos[workAxis2]++)
                        {
                            startVoxel = chunk.GetVoxel(startPos + chunk.ChunkPosition.VoxelPosition);

                            // If this voxel has already been merged, is air, or not visible skip it.
                            if (merged[startPos[workAxis1], startPos[workAxis2]] || startVoxel.ID == Voxel.AirId || !IsVoxelFaceVisible(startPos, direction, isBackFace)) {
                                continue;
                            }

                            // Reset the work var
                            quadSize = new Vector3Int();

                            // Figure out the width, then save it
                            for (currPos = startPos, currPos[workAxis2]++; currPos[workAxis2] < Dimensions[workAxis2] && CompareStep(startPos, currPos, direction, isBackFace) && !merged[currPos[workAxis1], currPos[workAxis2]]; currPos[workAxis2]++) { }
                            quadSize[workAxis2] = currPos[workAxis2] - startPos[workAxis2];

                            // Figure out the height, then save it
                            for (currPos = startPos, currPos[workAxis1]++; currPos[workAxis1] < Dimensions[workAxis1] && CompareStep(startPos, currPos, direction, isBackFace) && !merged[currPos[workAxis1], currPos[workAxis2]]; currPos[workAxis1]++) {
                                for (currPos[workAxis2] = startPos[workAxis2]; currPos[workAxis2] < Dimensions[workAxis2] && CompareStep(startPos, currPos, direction, isBackFace) && !merged[currPos[workAxis1], currPos[workAxis2]]; currPos[workAxis2]++) { }

                                // If we didn't reach the end then its not a good add.
                                if (currPos[workAxis2] - startPos[workAxis2] < quadSize[workAxis2]) {
                                    break;
                                }

                                currPos[workAxis2] = startPos[workAxis2];
                            }
                            quadSize[workAxis1] = currPos[workAxis1] - startPos[workAxis1];

                            // Now we add the quad to the mesh
                            m = new Vector3Int();
                            m[workAxis1] = quadSize[workAxis1];

                            n = new Vector3Int();
                            n[workAxis2] = quadSize[workAxis2];

                            // We need to add a slight offset when working with front faces.
                            offsetPos = startPos;
                            offsetPos[direction] += isBackFace ? 0 : 1;

                            //Draw the face to the mesh
                            
                            Vector3 a = offsetPos;
                            Vector3 b = offsetPos + m;
                            Vector3 c = offsetPos + m + n;
                            Vector3 d = offsetPos + n;
                            
                            Vector3 normal = Vector3.zero;
                            normal[direction] = isBackFace ? -1 : 1;

                            AddSquareFace(a, b, c, d, normal, startVoxel.Color, isBackFace);

                            // Mark it merged
                            for (int f = 0; f < quadSize[workAxis1]; f++) 
                            {
                                for (int g = 0; g < quadSize[workAxis2]; g++) 
                                {
                                    merged[startPos[workAxis1] + f, startPos[workAxis2] + g] = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddSquareFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, Color32 color, bool isBackFace)
        {
            // Add the 4 vertices, and color for each vertex.
            meshData.Vertices.Add(new Vertex(a, normal, color));
            meshData.Vertices.Add(new Vertex(b, normal, color));
            meshData.Vertices.Add(new Vertex(c, normal, color));
            meshData.Vertices.Add(new Vertex(d, normal, color));

            int i = vertexCount;
            vertexCount += 4;

            if (!isBackFace)
            {
                meshData.Triangles.Add((ushort)i);
                meshData.Triangles.Add((ushort)(i + 1));
                meshData.Triangles.Add((ushort)(i + 2));
                meshData.Triangles.Add((ushort)i);
                meshData.Triangles.Add((ushort)(i + 2));
                meshData.Triangles.Add((ushort)(i + 3));
            }
            else
            {
                meshData.Triangles.Add((ushort)(i + 2));
                meshData.Triangles.Add((ushort)(i + 1));
                meshData.Triangles.Add((ushort)i);
                meshData.Triangles.Add((ushort)(i + 3));
                meshData.Triangles.Add((ushort)(i + 2));
                meshData.Triangles.Add((ushort)i);
            }
        }

        private bool IsVoxelFaceVisible(Vector3Int voxelPosition, int axis, bool backFace) 
        {
            voxelPosition[axis] += backFace ? -1 : 1;
            voxelPosition += chunk.ChunkPosition.VoxelPosition;

            return chunk.GetVoxel(voxelPosition).ID == 0;
        }

        private bool CompareStep(Vector3Int a, Vector3Int b, int direction, bool backFace) 
        {
            a += chunk.ChunkPosition.VoxelPosition;
            b += chunk.ChunkPosition.VoxelPosition;

            Voxel voxel1 = chunk.GetVoxel(a);
            Voxel voxel2 = chunk.GetVoxel(b);

            return voxel1 == voxel2 && voxel2.ID != 0 && IsVoxelFaceVisible(b, direction, backFace);
        }
    }
}