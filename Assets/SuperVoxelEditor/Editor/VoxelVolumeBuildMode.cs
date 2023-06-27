namespace SuperVoxelEditor.Editor
{
    public abstract class VoxelVolumeBuildMode
    {
        public abstract void HandleMouseDown(VoxelVolumeEditor editor);
        
        public abstract void HandleMouseUp(VoxelVolumeEditor editor);

        public abstract void OnUpdate(VoxelVolumeEditor editor);
    }
}