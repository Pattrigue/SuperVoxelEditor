using System.Collections.Generic;
using UnityEngine.Rendering;

namespace SemagGames.VoxelEditor
{
    using UnityEngine;

    [ExecuteAlways]
    public sealed class ChunkMesh : MonoBehaviour
    {
        private const MeshUpdateFlags MeshUpdateFlags = UnityEngine.Rendering.MeshUpdateFlags.DontRecalculateBounds
                                                        | UnityEngine.Rendering.MeshUpdateFlags.DontValidateIndices
                                                        | UnityEngine.Rendering.MeshUpdateFlags.DontNotifyMeshUsers
                                                        | UnityEngine.Rendering.MeshUpdateFlags.DontResetBoneBounds;

        private static readonly Vector3Int Dimensions = new Vector3Int(16, 16, 16);
        
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
        
        private int vertexCount;

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

        public void Build()
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

        private void GenerateMeshData()
        {
            // This loop goes through each face of a voxel cube (6 faces)
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                bool isBackFace = faceIndex % 2 == 1;
                int primaryAxis = faceIndex % 3;
                int secondaryAxis = (primaryAxis + 1) % 3;
                int tertiaryAxis = (primaryAxis + 2) % 3;

                Vector3Int startPos = new Vector3Int();
                Vector3Int currentPos = new Vector3Int();

                // This loop iterates over each layer of the voxel chunk
                for (startPos[primaryAxis] = 0; startPos[primaryAxis] < Dimensions[primaryAxis]; startPos[primaryAxis]++)
                {
                    bool[,] hasMerged = new bool[Dimensions[secondaryAxis], Dimensions[tertiaryAxis]];

                    // Build the slices of the mesh.
                    for (startPos[secondaryAxis] = 0; startPos[secondaryAxis] < Dimensions[secondaryAxis]; startPos[secondaryAxis]++)
                    {
                        for (startPos[tertiaryAxis] = 0; startPos[tertiaryAxis] < Dimensions[tertiaryAxis]; startPos[tertiaryAxis]++)
                        {
                            Voxel currentVoxel = chunk.GetVoxelFromWorldPosition(startPos + chunk.ChunkPosition.VoxelPosition);

                            // If this voxel has already been merged, is air, or not visible, skip it.
                            if (hasMerged[startPos[secondaryAxis], startPos[tertiaryAxis]] || currentVoxel.ID == Voxel.AirId || !IsVoxelFaceVisible(startPos, primaryAxis, isBackFace))
                            {
                                continue;
                            }

                            // Determine the size of the quad to be added
                            Vector3Int quadSize = CalculateQuadSize(startPos, primaryAxis, secondaryAxis, tertiaryAxis, isBackFace, hasMerged);

                            // Add the quad to the mesh
                            AddQuadToMesh(startPos, quadSize, primaryAxis, secondaryAxis, tertiaryAxis, isBackFace, currentVoxel);

                            // Mark the voxels as merged
                            MarkVoxelsAsMerged(startPos, quadSize, hasMerged, secondaryAxis, tertiaryAxis);
                        }
                    }
                }
            }
        }

        private Vector3Int CalculateQuadSize(Vector3Int startPos, int primaryAxis, int secondaryAxis, int tertiaryAxis, bool isBackFace, bool[,] hasMerged)
        {
            // Initialize variables
            Vector3Int quadSize = new Vector3Int();
            Vector3Int currentPos = new Vector3Int();

            // Calculate the width of the quad
            for (currentPos = startPos, currentPos[tertiaryAxis]++; currentPos[tertiaryAxis] < Dimensions[tertiaryAxis] && CompareStep(startPos, currentPos, primaryAxis, isBackFace) && !hasMerged[currentPos[secondaryAxis], currentPos[tertiaryAxis]]; currentPos[tertiaryAxis]++) { }
            quadSize[tertiaryAxis] = currentPos[tertiaryAxis] - startPos[tertiaryAxis];

            // Calculate the height of the quad
            for (currentPos = startPos, currentPos[secondaryAxis]++; currentPos[secondaryAxis] < Dimensions[secondaryAxis] && CompareStep(startPos, currentPos, primaryAxis, isBackFace) && !hasMerged[currentPos[secondaryAxis], currentPos[tertiaryAxis]]; currentPos[secondaryAxis]++)
            {
                for (currentPos[tertiaryAxis] = startPos[tertiaryAxis]; currentPos[tertiaryAxis] < Dimensions[tertiaryAxis] && CompareStep(startPos, currentPos, primaryAxis, isBackFace) && !hasMerged[currentPos[secondaryAxis], currentPos[tertiaryAxis]]; currentPos[tertiaryAxis]++) { }

                // If the quad doesn't span the full width, it's not a good add.
                if (currentPos[tertiaryAxis] - startPos[tertiaryAxis] < quadSize[tertiaryAxis])
                {
                    break;
                }

                currentPos[tertiaryAxis] = startPos[tertiaryAxis];
            }
            quadSize[secondaryAxis] = currentPos[secondaryAxis] - startPos[secondaryAxis];

            return quadSize;
        }

        private void AddQuadToMesh(Vector3Int startPos, Vector3Int quadSize, int primaryAxis, int secondaryAxis, int tertiaryAxis, bool isBackFace, Voxel currentVoxel)
        {
            // Create the vectors for the corners of the quad
            Vector3Int horizontalVector = new Vector3Int();
            horizontalVector[secondaryAxis] = quadSize[secondaryAxis];

            Vector3Int verticalVector = new Vector3Int();
            verticalVector[tertiaryAxis] = quadSize[tertiaryAxis];

            // Offset the position if we're dealing with a front face
            Vector3Int offsetPosition = startPos;
            offsetPosition[primaryAxis] += isBackFace ? 0 : 1;

            // Create the vertices of the face
            Vector3 a = offsetPosition;
            Vector3 b = offsetPosition + horizontalVector;
            Vector3 c = offsetPosition + horizontalVector + verticalVector;
            Vector3 d = offsetPosition + verticalVector;

            // Calculate the normal for the face
            Vector3 normal = Vector3.zero;
            normal[primaryAxis] = isBackFace ? -1 : 1;

            // Add the square face to the mesh
            AddSquareFace(a, b, c, d, normal, currentVoxel.Color, isBackFace);
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
            voxelPosition += chunk.ChunkPosition.VoxelPosition; // Note - this is now a world position! (not a local voxel/chunk position)

            if (World.TryGetChunk(voxelPosition, out Chunk chunkAtPosition))
            {
                return chunkAtPosition.GetVoxelFromWorldPosition(voxelPosition).ID == Voxel.AirId;
            }

            return false;
        }

        private bool CompareStep(Vector3Int a, Vector3Int b, int direction, bool backFace) 
        {
            Voxel voxel1 = chunk.GetVoxel(a);
            Voxel voxel2 = chunk.GetVoxel(b);

            return voxel1 == voxel2 && voxel2.ID != Voxel.AirId && IsVoxelFaceVisible(b, direction, backFace);
        }
        
        private static void MarkVoxelsAsMerged(Vector3Int startPos, Vector3Int quadSize, bool[,] hasMerged, int secondaryAxis, int tertiaryAxis)
        {
            for (int i = 0; i < quadSize[secondaryAxis]; i++)
            {
                for (int j = 0; j < quadSize[tertiaryAxis]; j++)
                {
                    hasMerged[startPos[secondaryAxis] + i, startPos[tertiaryAxis] + j] = true;
                }
            }
        }
    }
}