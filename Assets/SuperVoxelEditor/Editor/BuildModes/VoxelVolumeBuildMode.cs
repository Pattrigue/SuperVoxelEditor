namespace SuperVoxelEditor.Editor.BuildModes
{
    public abstract class VoxelVolumeBuildMode
    {
        public abstract BuildMode BuildMode { get; }
        
        public abstract void HandleMouseDown(VoxelVolumeEditor editor);
        
        public abstract void HandleMouseUp(VoxelVolumeEditor editor);

        public abstract void OnUpdate(VoxelVolumeEditor editor);
    }
}