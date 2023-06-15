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
        }

        public void Build(IReadOnlyList<Voxel> voxels)
        {
            Debug.Log($"Building chunk mesh at {chunk.ChunkPosition}");

            ResetMesh();

            Vector3 chunkWorldPosition = chunk.ChunkPosition.WorldPosition;

            // for (int i = 0; i < Chunk.Size3D; i++)
            // {
            //     Voxel voxel = voxels[i];
            //
            //     if (voxel.ID == Voxel.AirId) continue;
            //
            //     Vector3Int voxelPosition = Chunk.ToVoxelPosition(i);
            //     Color color = voxel.Color;
            //     // Color32 baseColor = CalculateVoxelBaseColor(ref chunkWorldPosition, voxel.Asset.PrimaryColor, voxel.Asset.SecondaryColor, ref voxelPosition);
            //
            //     AddQuad(ref voxel, color, ref voxelPosition, Vector3Int.up, 0, 1, 2, 3);
            //     AddQuad(ref voxel, color, ref voxelPosition, Vector3Int.left, 1, 0, 4, 5);
            //     AddQuad(ref voxel, color, ref voxelPosition, Vector3Int.back, 0, 3, 7, 4);
            //     AddQuad(ref voxel, color, ref voxelPosition, Vector3Int.right, 3, 2, 6, 7);
            //     AddQuad(ref voxel, color, ref voxelPosition, Vector3Int.forward, 2, 1, 5, 6);
            //     AddQuad(ref voxel, color, ref voxelPosition, Vector3Int.down, 7, 6, 5, 4);
            // }

            // mesh.SetVertexBufferParams(test.Vertices.Count, VertexAttributeDescriptors);
            // mesh.SetVertexBufferData(test.Vertices.ToArray(), 0, 0, test.Vertices.Count, 0, MeshUpdateFlags);
            //
            // mesh.SetIndexBufferParams(test.Indices.Count, IndexFormat.UInt16);
            // mesh.SetIndexBufferData(test.Indices.ToArray(), 0, 0, test.Indices.Count, MeshUpdateFlags);
            //
            // mesh.SetSubMesh(0, new SubMeshDescriptor(0, test.Indices.Count), MeshUpdateFlags);

            var generatedMeshData = GenerateMesh();

            mesh.vertices = generatedMeshData.Vertices;
            mesh.triangles = generatedMeshData.Triangles;
            mesh.SetColors(generatedMeshData.Colors);
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;

            // meshData.Dispose();
        }

        private void ResetMesh()
        {
            // meshData.Dispose(); // ensure to dispose old one to avoid memory leak
            // meshData = MeshData.Allocate();

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

        private bool ShowFace(ref Vector3Int position, ref Vector3Int direction)
        {
            Vector3Int neighborPosition = position + direction;

            if (!Chunk.InChunkBounds(neighborPosition))
            {
                Vector3 worldPosition = this.chunk.ChunkPosition.WorldPosition + neighborPosition;

                if (World.TryGetChunk(worldPosition, out Chunk chunk) && chunk.HasVoxel(worldPosition)) return false;

                return true;
            }

            int neighborIndex = Chunk.GetVoxelIndex(neighborPosition.x, neighborPosition.y, neighborPosition.z);

            Voxel neighbor = chunk.Voxels[neighborIndex];

            return neighbor.ID == 0;
        }

        private static Color32 CalculateVoxelBaseColor(ref Vector3 chunkWorldPosition, Color32 primaryColor, Color32 secondaryColor, ref Vector3Int voxelPosition)
        {
            if (primaryColor.r == secondaryColor.r && primaryColor.g == secondaryColor.g && primaryColor.b == secondaryColor.b) return primaryColor;

            const float maxColorChange = 0.5f;
            const float gradientScale = 0.025f;

            float t = (1 + noise.snoise(new float3((chunkWorldPosition.x + voxelPosition.x) * gradientScale, (chunkWorldPosition.y + voxelPosition.y) * gradientScale, (chunkWorldPosition.z + voxelPosition.z) * gradientScale))) * maxColorChange;

            return Color32.Lerp(primaryColor, secondaryColor, t);
        }
        
        public static readonly Vector3Int Dimensions = new Vector3Int(16, 16, 16);
        
        public MeshData GenerateMesh()
        {
            MeshBuilder builder = new MeshBuilder();
            bool[,] merged;

            Vector3Int startPos, currPos, quadSize, m, n, offsetPos;
            Vector3[] vertices;

            Voxel startBlock;
            int direction, workAxis1, workAxis2;

            // Iterate over each face of the blocks.
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
                            startBlock = chunk.GetVoxel(startPos);

                            // If this block has already been merged, is air, or not visible skip it.
                            if (merged[startPos[workAxis1], startPos[workAxis2]] || startBlock.ID == 0 || !IsBlockFaceVisible(startPos, direction, isBackFace)) {
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
                                } else {
                                    currPos[workAxis2] = startPos[workAxis2];
                                }
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
                            vertices = new Vector3[] {
                                offsetPos,
                                offsetPos + m,
                                offsetPos + m + n,
                                offsetPos + n
                            };
                            builder.AddSquareFace(vertices, startBlock.Color, isBackFace);

                            // Mark it merged
                            for (int f = 0; f < quadSize[workAxis1]; f++) {
                                for (int g = 0; g < quadSize[workAxis2]; g++) {
                                    merged[startPos[workAxis1] + f, startPos[workAxis2] + g] = true;
                                }
                            }
                        }
                    }
                }
            }

            return builder.ToMeshData();
        }

        private bool IsBlockFaceVisible(Vector3Int blockPosition, int axis, bool backFace) 
        {
            blockPosition[axis] += backFace ? -1 : 1;

            return chunk.GetVoxel(blockPosition).ID == 0;
        }

        private bool CompareStep(Vector3Int a, Vector3Int b, int direction, bool backFace) 
        {
            Voxel blockA = chunk.GetVoxel(a);
            Voxel blockB = chunk.GetVoxel(b);

            return blockA == blockB && blockB.ID != 0 && IsBlockFaceVisible(b, direction, backFace);
        }
    }
}