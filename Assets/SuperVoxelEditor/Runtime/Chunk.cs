using System;
using System.Collections.Generic;
using UnityEngine;

namespace SemagGames.SuperVoxelEditor
{
    [RequireComponent(typeof(ChunkMesh), typeof(ChunkCollider))]
    [ExecuteAlways]
    public sealed class Chunk : MonoBehaviour
    {
        [SerializeField] private uint[] voxelData = new uint[Size3D];
        
        public const int Width = 16;
        public const int Height = 16;
        public const int Depth = 16;
        
        private const int Size3D = Width * Height * Depth;

        public static event Action<Chunk> Destroyed;

        public bool AutoRebuildCollider
        {
            get => autoRebuildCollider;
            set
            {
                if (autoRebuildCollider == value) return;

                if (!autoRebuildCollider && value)
                {
                    chunkCollider.Rebuild();
                }
                
                autoRebuildCollider = value;
            }
        }

        public VoxelVolume Volume { get; private set; }

        public ChunkPosition ChunkPosition => ChunkPosition.FromWorldPosition(transform.position);

        public IReadOnlyList<uint> VoxelData => voxelData;
        
        private static readonly Vector3[] NeighborDirections =
        {
            Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
        };

        private ChunkMesh mesh;
        private ChunkCollider chunkCollider;

        private bool isDirty;
        private bool autoRebuildCollider = true;

        private void Awake()
        {
            mesh = GetComponent<ChunkMesh>();
            chunkCollider = GetComponent<ChunkCollider>();
            Volume = GetComponentInParent<VoxelVolume>();
        }

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
        
        private void OnDestroy() => Destroyed?.Invoke(this);

        public void Rebuild()
        {
            mesh.Build();
            
            if (AutoRebuildCollider)
            {
                chunkCollider.Rebuild();
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
        }

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
        
        public void SetVoxel(Vector3Int localPosition, Voxel voxel)
        {
            SetVoxel(localPosition.x, localPosition.y, localPosition.z, voxel);
        }

        public bool HasVoxel(Vector3 worldPosition)
        {
            Vector3Int localPosition = ToLocalVoxelPosition(worldPosition);
            
            return HasVoxel(localPosition.x, localPosition.y, localPosition.z);
        }

        public void SetVoxel(int x, int y, int z, Voxel voxel)
        {
            int voxelIndex = GetVoxelIndex(x, y, z);

            voxelData[voxelIndex] = voxel.ToVoxelData();
            isDirty = true;
            
            MarkNeighborsDirtyIfOnEdge(x, y, z);
        }
        
        public uint GetVoxelData(Vector3Int localPosition)
        {
            return GetVoxelData(localPosition.x, localPosition.y, localPosition.z);
        }

        public uint GetVoxelData(int x, int y, int z)
        {
            if (!InChunkBounds(x, y, z)) return Voxel.AirVoxelData;
            
            return voxelData[GetVoxelIndex(x, y, z)];
        }

        public uint GetVoxelDataFromWorldPosition(Vector3 worldPosition)
        {
            Vector3Int localPosition = ToLocalVoxelPosition(worldPosition);
            
            return GetVoxelData(localPosition.x, localPosition.y, localPosition.z);
        }
        
        public Vector3Int ToLocalVoxelPosition(Vector3 worldPosition)
        {
            Vector3Int chunkVoxelPosition = ChunkPosition.VoxelPosition;
            
            return new Vector3Int(
                Mathf.FloorToInt(worldPosition.x) - chunkVoxelPosition.x,
                Mathf.FloorToInt(worldPosition.y) - chunkVoxelPosition.y,
                Mathf.FloorToInt(worldPosition.z) - chunkVoxelPosition.z
            );
        }
        
        public bool HasVoxel(int x, int y, int z)
        {
            return !Voxel.IsAir(GetVoxelData(x, y, z));
        }

        public static bool InChunkBounds(int x, int y, int z)
        {
            return x is >= 0 and < Width && y is >= 0 and < Height && z is >= 0 and < Depth;
        }

        public static bool InChunkBounds(Vector3Int voxelPosition)
        {
            return InChunkBounds(voxelPosition.x, voxelPosition.y, voxelPosition.z);
        }

        private void MarkNeighborsDirtyIfOnEdge(int x, int y, int z)
        {
            // Check if the voxel is on the edge of the chunk
            if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1 || z == 0 || z == Depth - 1)
            {
                for (int i = 0; i < NeighborDirections.Length; i++)
                {
                    // Check if the voxel is on the edge of the chunk in the current direction
                    if (Volume.TryGetChunk(transform.position + NeighborDirections[i], out Chunk chunk))
                    {
                        chunk.isDirty = true;
                    }
                }
            }
        }

        private static int GetVoxelIndex(int x, int y, int z) => x + Width * (y + Height * z);
    }
}