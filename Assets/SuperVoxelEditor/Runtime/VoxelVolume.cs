using System.Collections.Generic;
using UnityEngine;

namespace SemagGames.SuperVoxelEditor
{
    [ExecuteAlways]
    public sealed class VoxelVolume : MonoBehaviour
    {
        [SerializeField] private VoxelProperty voxelProperty;
        [SerializeField] private ColorPicker colorPicker;
        [SerializeField] private Chunk chunkPrefab;
    
        public ColorPicker ColorPicker => colorPicker;

        public VoxelProperty VoxelProperty
        {
            get => voxelProperty;
            set => voxelProperty = value; 
        }

        public IEnumerable<Chunk> Chunks => chunks.Values;

        private readonly Dictionary<ChunkPosition, Chunk> chunks = new();
    
        private void OnEnable()
        {
            chunks.Clear();

            foreach (Chunk chunk in GetComponentsInChildren<Chunk>())
            {
                chunks.Add(chunk.ChunkPosition, chunk);
            }
            
            Chunk.Destroyed += OnChunkDestroyed;
        }

        private void OnDisable()
        {
            Chunk.Destroyed -= OnChunkDestroyed;
        }

        public void Clear()
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            chunks.Clear();
        }

        public void SetVoxel(Vector3 worldPosition, uint colorId = 0, uint voxelPropertyId = 1)
        {
            ChunkPosition chunkPosition = ChunkPosition.FromWorldPosition(worldPosition);
        
            if (!chunks.TryGetValue(chunkPosition, out Chunk chunk))
            {
                chunk = Instantiate(chunkPrefab, chunkPosition.WorldPosition, Quaternion.identity, transform);
                chunk.name = $"Chunk {chunkPosition}";

                chunks.Add(chunkPosition, chunk);
            }

            chunk.SetVoxel(worldPosition, new Voxel(voxelPropertyId, colorId));
        }

        public uint GetVoxelData(Vector3 worldPosition)
        {
            ChunkPosition chunkPosition = ChunkPosition.FromWorldPosition(worldPosition);
        
            if (!chunks.TryGetValue(chunkPosition, out Chunk chunk))
            {
                return Voxel.AirVoxelData;
            }
            
            return chunk.GetVoxelDataFromWorldPosition(worldPosition);
        }
        
        public bool TryGetVoxel(Vector3 worldPosition, out Voxel voxel)
        {
            var voxelData = GetVoxelData(worldPosition);
            
            if (Voxel.IsAir(voxelData))
            {
                voxel = default;
                return false;
            }
            
            voxel = Voxel.FromVoxelData(voxelData);
            return true;
        }
        
        public bool TryGetChunk(Vector3 position, out Chunk chunk)
        {
            ChunkPosition chunkPosition = ChunkPosition.FromWorldPosition(position);

            return chunks.TryGetValue(chunkPosition, out chunk);
        }

        public bool TryGetChunk(ChunkPosition chunkPosition, out Chunk chunk)
        {
            return chunks.TryGetValue(chunkPosition, out chunk);
        }
        
        private void OnChunkDestroyed(Chunk destroyedChunk)
        {
            if (chunks.ContainsKey(destroyedChunk.ChunkPosition))
            {
                chunks.Remove(destroyedChunk.ChunkPosition);
            }
        }
    }
}