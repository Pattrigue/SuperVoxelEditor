using UnityEngine;

namespace SemagGames.VoxelEditor
{
    public sealed class VoxelTerrainTester : MonoBehaviour
    {
        [SerializeField] private float scale = 0.1f;
        [SerializeField] private int octaves = 2; // Number of octaves
        [SerializeField] private float persistence = 5f; // Persistence of noise
        [SerializeField] private float lacunarity = 0.5f; // Frequency multiplier per octave
        [SerializeField] private float exponentiation = 1.5f; // Exponent for height
        [SerializeField] private float heightMultiplier = 1f; // Multiplier for height

        [SerializeField] private int numChunks = 4;
        [SerializeField] private VoxelVolume volume;
        [SerializeField] private VoxelProperty grassVoxel;
        [SerializeField] private VoxelProperty stoneVoxel;

        private void Start()
        {
            Rebuild();
        }

        public void Rebuild()
        {
            volume.Clear();

            for (int x = 0; x < Chunk.Width * numChunks; x++)
            for (int z = 0; z < Chunk.Depth * numChunks; z++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float perlinValue = Mathf.PerlinNoise(x * scale * frequency, z * scale * frequency);
                    noiseHeight += Mathf.Abs(perlinValue) * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                noiseHeight = Mathf.Pow(noiseHeight, exponentiation) * heightMultiplier;

                int height = Mathf.Min(Chunk.Height, Mathf.FloorToInt(noiseHeight));

                for (int y = 0; y < Chunk.Height; y++)
                {
                    uint voxelPropertyId;
                    uint colorId;

                    if (y <= height)
                    {
                        voxelPropertyId = y == height ? grassVoxel.ID : stoneVoxel.ID;
                        colorId = y == height ? 167u : 6u;
                    }
                    else
                    {
                        voxelPropertyId = VoxelProperty.AirId;
                        colorId = 0;
                    }

                    volume.SetVoxel(new Vector3(x, y, z), colorId, voxelPropertyId);
                }
            }
        }
    }
}