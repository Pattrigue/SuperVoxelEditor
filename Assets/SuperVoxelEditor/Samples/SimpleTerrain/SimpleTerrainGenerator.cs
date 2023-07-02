using UnityEngine;

namespace SemagGames.SuperVoxelEditor.Samples.SimpleTerrain
{
    public sealed class SimpleTerrainGenerator : MonoBehaviour
    {
        [SerializeField] private float scale = 0.1f;
        [SerializeField] private int octaves = 2; // Number of octaves
        [SerializeField] private float persistence = 5f; // Persistence of noise
        [SerializeField] private float lacunarity = 0.5f; // Frequency multiplier per octave
        [SerializeField] private float exponentiation = 1.5f; // Exponent for height
        [SerializeField] private float heightMultiplier = 1f; // Multiplier for height

        [SerializeField] private int numChunks = 4;
        [SerializeField] private VoxelVolume volume;
        [SerializeField] private VoxelAsset grassVoxel;
        [SerializeField] private VoxelAsset stoneVoxel;

        private void Start()
        {
            Rebuild();
        }

        public void Rebuild()
        {
            volume.Clear();

            for (int x = 0; x < Chunk.Width * numChunks; x++)
            {
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
                        if (y <= height)
                        {
                            Vector3 pos = new Vector3(x, y, z);
                            
                            VoxelAsset voxelAsset = y == height ? grassVoxel : stoneVoxel;
                            volume.SetVoxel(pos, voxelAsset);

                            if (voxelAsset == grassVoxel && Random.Range(0, 100) < 50)
                            {
                                // Add some random color variation to the grass
                                Color color = grassVoxel.Color + new Color(0, Random.Range(0, 0.02f), 0);
                                volume.SetVoxelColor(pos, color);
                            }
                        }
                    }
                }
            }
        }
    }
}