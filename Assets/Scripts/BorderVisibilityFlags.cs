namespace SemagGames.VoxelEditor
{
    [System.Flags]
    public enum BorderVisibilityFlags
    {
        None = 0,
        Left = 1, // Axis 0, backFace
        Right = 2, // Axis 0, !backFace
        Bottom = 4, // Axis 1, backFace
        Top = 8, // Axis 1, !backFace
        Front = 16, // Axis 2, backFace
        Back = 32, // Axis 2, !backFace
        Everything = Left | Right | Bottom | Top | Front | Back
    }
}