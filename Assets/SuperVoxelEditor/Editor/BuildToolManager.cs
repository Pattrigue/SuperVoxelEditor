﻿namespace SuperVoxelEditor.Editor
{
    public sealed class BuildToolManager
    {
        public BuildToolInputManager Input { get; }
        public BuildToolInspector Inspector { get; }
        
        public BuildToolType SelectedTool => Inspector.SelectedTool;
        
        public BuildToolManager()
        {
            Inspector = new BuildToolInspector();
            Input = new BuildToolInputManager(Inspector);
        }
    }
}