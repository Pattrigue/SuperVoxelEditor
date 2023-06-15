namespace SemagGames.VoxelEditor
{
    public sealed class MeshData
    {
        public Vertex[] Vertices { get; }
        public ushort[] Triangles { get; }

        public MeshData(Vertex[] vertices, ushort[] triangles)
        {
            Vertices = vertices;
            Triangles = triangles;
        }
    }
}