using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace SemagGames.VoxelEditor
{
    public struct MeshData
    {
        private static readonly AllocatorManager.AllocatorHandle AllocatorHandle = Allocator.Persistent;

        [NativeDisableContainerSafetyRestriction]
        public NativeList<Vertex> Vertices;

        [NativeDisableContainerSafetyRestriction]
        public NativeList<ushort> Indices;

        public static MeshData Allocate()
        {
            NativeList<Vertex> vertices = new(AllocatorHandle);
            NativeList<ushort> indices = new(AllocatorHandle);

            return new MeshData(ref vertices, ref indices);
        }

        private MeshData(ref NativeList<Vertex> vertices, ref NativeList<ushort> indices)
        {
            Vertices = vertices;
            Indices = indices;
        }

        public void Clear()
        {
            if (Vertices.IsCreated) Vertices.Clear();

            if (Indices.IsCreated) Indices.Clear();
        }

        public void Dispose()
        {
            if (Vertices.IsCreated) Vertices.Dispose();

            if (Indices.IsCreated) Indices.Dispose();
        }
    }
}