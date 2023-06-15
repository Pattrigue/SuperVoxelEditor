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
        public NativeList<ushort> Triangles;

        public static MeshData Allocate()
        {
            NativeList<Vertex> vertices = new(AllocatorHandle);
            NativeList<ushort> indices = new(AllocatorHandle);

            return new MeshData(ref vertices, ref indices);
        }

        private MeshData(ref NativeList<Vertex> vertices, ref NativeList<ushort> triangles)
        {
            Vertices = vertices;
            Triangles = triangles;
        }

        public void Clear()
        {
            if (Vertices.IsCreated) Vertices.Clear();
            if (Triangles.IsCreated) Triangles.Clear();
        }

        public void Dispose()
        {
            if (Vertices.IsCreated) Vertices.Dispose();
            if (Triangles.IsCreated) Triangles.Dispose();
        }
    }
}