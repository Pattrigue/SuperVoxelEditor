using SuperVoxelEditor.Editor.BuildModes;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class BuildModeController
    {
        public VoxelVolumeBuildMode ActiveBuildMode { get; private set; }
        
        private BuildMode currentMode;

        public BuildModeController()
        {
            ActiveBuildMode = new VoxelBuildMode(); // default build mode
            currentMode = BuildMode.Voxel;
        }

        public void SwitchBuildMode(BuildMode newMode)
        {
            if (newMode == currentMode) return;

            switch (newMode)
            {
                case BuildMode.Voxel:
                    ActiveBuildMode = new VoxelBuildMode();
                    break;
                case BuildMode.Box:
                    ActiveBuildMode = new BoxBuildMode();
                    break;
                default:
                    Debug.LogError($"Invalid BuildMode: {newMode}");
                    break;
            }

            currentMode = newMode;
        }
    }
}