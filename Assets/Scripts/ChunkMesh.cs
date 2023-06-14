using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace SemagGames.VoxelEditor
{
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
            meshData.Dispose();
        }

        public void Build(IReadOnlyList<Voxel> voxels)
        {
            Debug.Log($"Building chunk mesh at {chunk.ChunkPosition}");

            ResetMesh();

            Vector3 chunkWorldPosition = chunk.ChunkPosition.WorldPosition;

            for (int i = 0; i < Chunk.Size3D; i++)
            {
                Voxel voxel = voxels[i];

                if (voxel.ID == Voxel.AirId) continue;

                Vector3Int voxelPosition = Chunk.ToVoxelPosition(i);
                Color color = voxel.Color;
                // Color32 baseColor = CalculateVoxelBaseColor(ref chunkWorldPosition, voxel.Asset.PrimaryColor, voxel.Asset.SecondaryColor, ref voxelPosition);

                AddQuad(ref voxel, color, ref voxelPosition, Vector3Int.up, 0, 1, 2, 3);
                AddQuad(ref voxel, color, ref voxelPosition, Vector3Int.left, 1, 0, 4, 5);
                AddQuad(ref voxel, color, ref voxelPosition, Vector3Int.back, 0, 3, 7, 4);
                AddQuad(ref voxel, color, ref voxelPosition, Vector3Int.right, 3, 2, 6, 7);
                AddQuad(ref voxel, color, ref voxelPosition, Vector3Int.forward, 2, 1, 5, 6);
                AddQuad(ref voxel, color, ref voxelPosition, Vector3Int.down, 7, 6, 5, 4);
            }

            mesh.SetVertexBufferParams(meshData.Vertices.Length, VertexAttributeDescriptors);
            mesh.SetVertexBufferData(meshData.Vertices.AsArray(), 0, 0, meshData.Vertices.Length, 0, MeshUpdateFlags);

            mesh.SetIndexBufferParams(meshData.Indices.Length, IndexFormat.UInt16);
            mesh.SetIndexBufferData(meshData.Indices.AsArray(), 0, 0, meshData.Indices.Length, MeshUpdateFlags);

            mesh.SetSubMesh(0, new SubMeshDescriptor(0, meshData.Indices.Length), MeshUpdateFlags);

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

        private void AddQuad(ref Voxel voxel, Color32 baseColor, ref Vector3Int voxelPosition, Vector3Int direction, int ai, int bi, int ci, int di)
        {
            if (!ShowFace(ref voxelPosition, ref direction)) return;

            int i = vertexCount;
            vertexCount += 4;

            Vector3 origin = voxelPosition + new Vector3(0.5f, 0.5f, 0.5f);

            Vector3 a = origin + VertexPositions[ai];
            Vector3 b = origin + VertexPositions[bi];
            Vector3 c = origin + VertexPositions[ci];
            Vector3 d = origin + VertexPositions[di];

            Vector3 normal = direction;

            Random.State state = Random.state;

            Random.InitState(voxelPosition.x * 10000 + voxelPosition.y * 100 + voxelPosition.z);

            Random.state = state;

            meshData.Vertices.Add(new Vertex(a, normal, baseColor));
            meshData.Vertices.Add(new Vertex(b, normal, baseColor));
            meshData.Vertices.Add(new Vertex(c, normal, baseColor));
            meshData.Vertices.Add(new Vertex(d, normal, baseColor));

            meshData.Indices.Add((ushort)i);
            meshData.Indices.Add((ushort)(i + 1));
            meshData.Indices.Add((ushort)(i + 2));
            meshData.Indices.Add((ushort)i);
            meshData.Indices.Add((ushort)(i + 2));
            meshData.Indices.Add((ushort)(i + 3));
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
    }
}