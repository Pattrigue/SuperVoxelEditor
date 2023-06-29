using System.Collections.Generic;
using SemagGames.SuperVoxelEditor;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class ChunkBoundsDrawer
    {
        private readonly IEnumerable<Chunk> chunks;
        private readonly Color chunkColor;

        public ChunkBoundsDrawer(IEnumerable<Chunk> chunks, Color chunkColor)
        {
            this.chunks = chunks;
            this.chunkColor = chunkColor;
        }

        public void Draw()
        {
            foreach (Chunk chunk in chunks)
            {
                Vector3 size = new Vector3(Chunk.Width, Chunk.Height, Chunk.Width);
                Vector3 center = chunk.transform.position + size * 0.5f;

                Color handlesColor = Handles.color;

                Handles.color = chunkColor;
                Handles.DrawWireCube(center, size);
                Handles.color = handlesColor;
            }
        }
    }
}