using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public sealed class World : Singleton<World>
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
    }

    public static void Clear()
    {
        while (Instance.transform.childCount > 0)
        {
            DestroyImmediate(Instance.transform.GetChild(0).gameObject);
        }

        Instance.chunks.Clear();
    }

    public static void SetVoxel(Vector3 position, uint voxelId)
    {
        ChunkPosition chunkPosition = ChunkPosition.FromWorldPosition(position);
        
        if (!Instance.chunks.TryGetValue(chunkPosition, out Chunk chunk))
        {
            chunk = Instantiate(Instance.chunkPrefab, chunkPosition.WorldPosition, Quaternion.identity, Instance.transform);
            chunk.name = $"Chunk {chunkPosition}";

            Instance.chunks.Add(chunkPosition, chunk);
        }

        chunk.SetVoxel(position, new Voxel(voxelId, Instance.colorPicker.SelectedColor));
    }

    public static bool TryGetChunk(Vector3 position, out Chunk chunk)
    {
        ChunkPosition chunkPosition = ChunkPosition.FromWorldPosition(position);

        return Instance.chunks.TryGetValue(chunkPosition, out chunk);
    }

    public static bool TryGetChunk(ChunkPosition chunkPosition, out Chunk chunk)
    {
        return Instance.chunks.TryGetValue(chunkPosition, out chunk);
    }
}