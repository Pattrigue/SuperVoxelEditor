namespace SuperVoxelEditor.Editor
{
    public abstract class BuildMode
    {
        public abstract void HandleMouseDown(VoxelEditorContext ctx);
        
        public abstract void HandleMouseUp(VoxelEditorContext ctx);

        public abstract void UpdatePreview(VoxelEditorContext ctx);
    }
}