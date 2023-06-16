using System.Collections.Generic;
using SemagGames.VoxelEditor.ColorPicking;
using UnityEngine;

namespace SemagGames.VoxelEditor
{
    [ExecuteAlways]
    public sealed class World : MonoBehaviour
    {
        [SerializeField] private Chunk chunkPrefab;
        [SerializeField] private ColorPicker colorPicker;
    
        public ColorPicker ColorPicker => colorPicker;

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

        public void SetVoxel(Vector3 worldPosition, uint voxelId)
        {
            ChunkPosition chunkPosition = ChunkPosition.FromWorldPosition(worldPosition);
        
            if (!chunks.TryGetValue(chunkPosition, out Chunk chunk))
            {
                chunk = Instantiate(chunkPrefab, chunkPosition.WorldPosition, Quaternion.identity, transform);
                chunk.name = $"Chunk {chunkPosition}";

                chunks.Add(chunkPosition, chunk);
            }

            chunk.SetVoxel(worldPosition, new Voxel(voxelId, colorPicker.SelectedColor));
        }

        public Voxel GetVoxel(Vector3 worldPosition)
        {
            ChunkPosition chunkPosition = ChunkPosition.FromWorldPosition(worldPosition);
        
            if (!chunks.TryGetValue(chunkPosition, out Chunk chunk))
            {
                return Voxel.Air;
            }
            
            return chunk.GetVoxelFromWorldPosition(worldPosition);
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