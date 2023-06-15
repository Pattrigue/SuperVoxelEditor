namespace SemagGames.VoxelEditor
{
    public sealed class MeshData
    {
        public Vertex[] Vertices { get; }
        public int[] Triangles { get; }

        public MeshData(Vertex[] vertices, int[] triangles)
        {
            Vertices = vertices;
            Triangles = triangles;
        }
    }
}