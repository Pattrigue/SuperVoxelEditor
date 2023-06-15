using System.Collections.Generic;
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
        
        private const int Size3D = Width * Height * Depth;

        public IReadOnlyList<Voxel> Voxels => voxels;

        public ChunkPosition ChunkPosition => ChunkPosition.FromWorldPosition(transform.position);

        private static readonly Vector3[] NeighborDirections =
        {
            Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
        };

        private ChunkMesh mesh;
        
        private bool isDirty;

        private void Awake() => mesh = GetComponent<ChunkMesh>();
        
        private void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += Update;
#endif
        }
        
        private void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= Update;
#endif
        }

        public void Rebuild() => mesh.Build(Voxels);

        private void Update()
        {
            if (isDirty)
            {
                Rebuild();
                isDirty = false;
            }
        }

        public void SetVoxel(Vector3 worldPosition, Voxel voxel)
        {
            Vector3Int localPosition = ToLocalVoxelPosition(worldPosition);
            
            SetVoxel(localPosition.x, localPosition.y, localPosition.z, voxel);
        }

        public Voxel GetVoxel(Vector3 worldPosition)
        {
            Vector3Int localPosition = ToLocalVoxelPosition(worldPosition);
            
            return GetVoxel(localPosition.x, localPosition.y, localPosition.z);
        }

        public bool HasVoxel(Vector3 worldPosition)
        {
            Vector3Int localPosition = ToLocalVoxelPosition(worldPosition);
            
            return HasVoxel(localPosition.x, localPosition.y, localPosition.z);
        }

        public void SetVoxel(int x, int y, int z, Voxel voxel)
        {
            int voxelIndex = GetVoxelIndex(x, y, z);
            
            voxels[voxelIndex] = voxel;
            isDirty = true;
            
            MarkNeighborsDirtyIfOnEdge(x, y, z);
        }

        public Voxel GetVoxel(int x, int y, int z)
        {
            if (!InChunkBounds(x, y, z)) return Voxel.Air;
            return voxels[GetVoxelIndex(x, y, z)];
        }

        public bool HasVoxel(int x, int y, int z)
        {
            return GetVoxel(x, y, z).ID != Voxel.AirId;
        }

        public static bool InChunkBounds(int x, int y, int z)
        {
            return x is >= 0 and < Width && y is >= 0 and < Height && z is >= 0 and < Depth;
        }

        public static bool InChunkBounds(Vector3Int voxelPosition)
        {
            return InChunkBounds(voxelPosition.x, voxelPosition.y, voxelPosition.z);
        }

        private Vector3Int ToLocalVoxelPosition(Vector3 worldPosition)
        {
            Vector3Int chunkVoxelPosition = ChunkPosition.VoxelPosition;
            
            return new Vector3Int(
                Mathf.FloorToInt(worldPosition.x) - chunkVoxelPosition.x,
                Mathf.FloorToInt(worldPosition.y) - chunkVoxelPosition.y,
                Mathf.FloorToInt(worldPosition.z) - chunkVoxelPosition.z
            );
        }

        private void MarkNeighborsDirtyIfOnEdge(int x, int y, int z)
        {
            // Check if the voxel is on the edge of the chunk
            if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1 || z == 0 || z == Depth - 1)
            {
                for (int i = 0; i < NeighborDirections.Length; i++)
                {
                    // Check if the voxel is on the edge of the chunk in the current direction
                    if (World.TryGetChunk(transform.position + NeighborDirections[i], out Chunk chunk))
                    {
                        chunk.isDirty = true;
                    }
                }
            }
        }

        private static int GetVoxelIndex(int x, int y, int z) => x + Width * (y + Height * z);
    }
}