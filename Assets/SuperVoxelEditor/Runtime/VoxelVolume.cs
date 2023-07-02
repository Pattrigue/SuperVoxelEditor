using System.Collections.Generic;
using SemagGames.SuperVoxelEditor.Commands;
using UnityEngine;

namespace SemagGames.SuperVoxelEditor
{
    [ExecuteAlways]
    public sealed class VoxelVolume : MonoBehaviour
    {
        [SerializeField] private VoxelAsset voxelAsset;
        [SerializeField] private Chunk chunkPrefab;
        [SerializeField] private CommandManager commandManager = new();

        public VoxelAsset VoxelAsset
        {
            get => voxelAsset;
            set => voxelAsset = value; 
        }

        public bool AutoRebuildChunkColliders
        {
            get => autoRebuildChunkColliders;
            set
            {
                if (autoRebuildChunkColliders == value) return;
                
                foreach (Chunk chunk in chunks.Values)
                {
                    chunk.AutoRebuildCollider = value;
                }

                autoRebuildChunkColliders = value;
            }
        }
        
        public IEnumerable<Chunk> Chunks => chunks.Values;
        public CommandManager Commands => commandManager;

        private readonly Dictionary<ChunkPosition, Chunk> chunks = new();

        private bool autoRebuildChunkColliders = true;
        
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
        
        public void SetVoxel(Vector3 worldPosition, Voxel voxel)
        {
            Chunk chunk = GetOrCreateChunk(worldPosition);

            SetVoxelCommand command = new SetVoxelCommand(chunk, worldPosition, voxel);
            commandManager.Do(command);
        }
        
        public void SetVoxel(Vector3 worldPosition, VoxelAsset voxelAsset)
        {
            SetVoxel(worldPosition, new Voxel(voxelAsset));
        }
        
        public void SetVoxel(Vector3 worldPosition, VoxelAsset voxelAsset, Color32 color)
        {
            SetVoxel(worldPosition, new Voxel(voxelAsset.ID, color));
        }
        
        public void SetVoxels(Vector3[] worldPositions, Voxel voxel)
        {
            SetVoxelBatchCommand command = new SetVoxelBatchCommand(this, worldPositions, voxel);
            commandManager.Do(command);
        }

        public void SetVoxels(Vector3[] worldPositions, VoxelAsset voxelAsset)
        {
            SetVoxels(worldPositions, new Voxel(voxelAsset));
        }
        
        public void SetVoxels(Vector3[] worldPositions, VoxelAsset voxelAsset, Color32 color)
        {
            SetVoxels(worldPositions, new Voxel(voxelAsset.ID, color));
        }
        
        public void SetVoxelColor(Vector3 worldPosition, Color32 color)
        {
            Voxel voxel = GetVoxel(worldPosition);
            voxel.SetColor(color);
            
            SetVoxel(worldPosition, voxel);
        }

        public void EraseVoxel(Vector3 worldPosition) => SetVoxel(worldPosition, Voxel.Air);
        
        public void EraseVoxels(Vector3[] worldPositions) => SetVoxels(worldPositions, Voxel.Air);

        public Chunk GetOrCreateChunk(Vector3 worldPosition)
        {
            ChunkPosition chunkPosition = ChunkPosition.FromWorldPosition(worldPosition);

            if (!chunks.TryGetValue(chunkPosition, out Chunk chunk))
            {
                chunk = Instantiate(chunkPrefab, chunkPosition.WorldPosition, Quaternion.identity, transform);
                chunk.name = $"Chunk {chunkPosition}";
                chunk.AutoRebuildCollider = autoRebuildChunkColliders;

                chunks.Add(chunkPosition, chunk);
            }

            return chunk;
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
        
        public Voxel GetVoxel(Vector3 worldPosition)
        {
            uint voxelData = GetVoxelData(worldPosition);
            
            return Voxel.FromVoxelData(voxelData);
        }
        
        public bool TryGetVoxel(Vector3 worldPosition, out Voxel voxel)
        {
            uint voxelData = GetVoxelData(worldPosition);
            
            if (Voxel.IsAir(voxelData))
            {
                voxel = default;
                return false;
            }
            
            voxel = Voxel.FromVoxelData(voxelData);
            return true;
        }
        
        public bool HasVoxel(Vector3 worldPosition)
        {
            uint voxelData = GetVoxelData(worldPosition);
            return !Voxel.IsAir(voxelData);
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

        public void Undo() => commandManager.Undo();

        public void Redo() => commandManager.Redo();
    }
}