using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SemagGames.VoxelEditor
{
    [RequireComponent(typeof(ChunkMesh))]
    [ExecuteAlways]
    public sealed class Chunk : MonoBehaviour
    {
        [SerializeField] private Voxel[] voxels = new Voxel[Size3D];

        public const int Width = 16;
        public const int Height = 16;
        public const int Depth = 16;

        public const int Size3D = Width * Height * Depth;

        private static readonly Vector3[] NeighborDirections =
        {
            Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
        };

        public ChunkPosition ChunkPosition => ChunkPosition.FromWorldPosition(transform.position);

        public IReadOnlyList<Voxel> Voxels => voxels;

        private ChunkMesh mesh;

        private bool isDirty;

        private void Awake()
        {
            mesh = GetComponent<ChunkMesh>();
        }

        public void Rebuild()
        {
            mesh.Build(Voxels);
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.update += LateUpdate;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= LateUpdate;
#endif
        }

        private void LateUpdate()
        {
            if (isDirty)
            {
                mesh.Build(Voxels);
                isDirty = false;
            }
        }

        public void SetVoxel(Vector3 worldPosition, Voxel voxel)
        {
            Vector3Int chunkVoxelPosition = ChunkPosition.VoxelPosition;

            int x = Mathf.FloorToInt(worldPosition.x) - chunkVoxelPosition.x;
            int y = Mathf.FloorToInt(worldPosition.y) - chunkVoxelPosition.y;
            int z = Mathf.FloorToInt(worldPosition.z) - chunkVoxelPosition.z;

            int voxelIndex = GetVoxelIndex(x, y, z);

            voxels[voxelIndex] = voxel;
            isDirty = true;
        
            // check if the voxel is on the edge of the chunk
            if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1 || z == 0 || z == Depth - 1)
            {
                // Iterate over the six possible neighbor directions
                for (int i = 0; i < NeighborDirections.Length; i++)
                {
                    // Check if the voxel is on the edge of the chunk in the current direction
                    if (World.TryGetChunk(worldPosition + NeighborDirections[i], out Chunk chunk))
                    {
                        chunk.isDirty = true;
                    }
                }
            }
        }

        public bool HasVoxel(Vector3 worldPosition)
        {
            Vector3Int chunkVoxelPosition = ChunkPosition.VoxelPosition;

            int x = Mathf.FloorToInt(worldPosition.x) - chunkVoxelPosition.x;
            int y = Mathf.FloorToInt(worldPosition.y) - chunkVoxelPosition.y;
            int z = Mathf.FloorToInt(worldPosition.z) - chunkVoxelPosition.z;

            return HasVoxel(x, y, z);
        }

        public bool HasVoxel(int x, int y, int z)
        {
            return voxels[GetVoxelIndex(x, y, z)].ID != Voxel.AirId;
        }

        public static int GetVoxelIndex(int x, int y, int z)
        {
            return x + Width * (y + Height * z);
        }

        public static Vector3Int ToVoxelPosition(int voxelIndex)
        {
            return new Vector3Int(voxelIndex % Width, voxelIndex / Width % Height, voxelIndex / (Width * Height));
        }

        public static bool InChunkBounds(int x, int y, int z)
        {
            return x is >= 0 and < Width && y is >= 0 and < Height && z is >= 0 and < Depth;
        }

        public static bool InChunkBounds(Vector3Int voxelPosition)
        {
            return InChunkBounds(voxelPosition.x, voxelPosition.y, voxelPosition.z);
        }
    }
}