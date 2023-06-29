namespace SuperVoxelEditor.Editor.BuildTools
{
    public sealed class BuildToolController
    {
        public BuildToolInputManager Input { get; }
        public BuildToolInspector Inspector { get; }
        
        public BuildTool SelectedTool => Inspector.SelectedTool;
        
        public BuildToolController()
        {
            Inspector = new BuildToolInspector();
            Input = new BuildToolInputManager(Inspector);
        }
    }
}