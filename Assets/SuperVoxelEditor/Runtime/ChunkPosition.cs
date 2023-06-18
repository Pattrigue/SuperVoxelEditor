using System;
using UnityEngine;

namespace SemagGames.SuperVoxelEditor
{
    public struct ChunkPosition
    {
        public static readonly ChunkPosition Zero = new(0, 0, 0);
        public static readonly ChunkPosition Left = new(-1, 0, 0);
        public static readonly ChunkPosition Right = new(1, 0, 0);
        public static readonly ChunkPosition Up = new(0, 1, 0);
        public static readonly ChunkPosition Down = new(0, -1, 0);
        public static readonly ChunkPosition Forward = new(0, 0, 1);
        public static readonly ChunkPosition Backward = new(0, 0, -1);

        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public Vector3 WorldPosition => new(X * Chunk.Width, Y * Chunk.Height, Z * Chunk.Depth);

        public Vector3Int VoxelPosition => new(
            Mathf.FloorToInt(X * Chunk.Width),
            Mathf.FloorToInt(Y * Chunk.Height),
            Mathf.FloorToInt(Z * Chunk.Depth)
        );

        public ChunkPosition(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static ChunkPosition FromWorldPosition(Vector3 worldPosition)
        {
            return new ChunkPosition(
                Mathf.FloorToInt(worldPosition.x / Chunk.Width),
                Mathf.FloorToInt(worldPosition.y / Chunk.Height),
                Mathf.FloorToInt(worldPosition.z / Chunk.Depth)
            );
        }

        public static ChunkPosition FromWorldPosition(float worldX, float worldY, float worldZ)
        {
            return FromWorldPosition(new Vector3(worldX, worldY, worldZ));
        }

        public static ChunkPosition operator +(ChunkPosition a, ChunkPosition b)
        {
            return new ChunkPosition(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static ChunkPosition operator -(ChunkPosition a, ChunkPosition b)
        {
            return new ChunkPosition(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static bool operator ==(ChunkPosition a, ChunkPosition b)
        {
            return a.X == b.X && a.Z == b.Z;
        }

        public static bool operator !=(ChunkPosition a, ChunkPosition b)
        {
            return !(a == b);
        }

        public bool Equals(ChunkPosition other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public override string ToString()
        {
            return $"ChunkPosition({X}, {Y}, {Z})";
        }
    }
}