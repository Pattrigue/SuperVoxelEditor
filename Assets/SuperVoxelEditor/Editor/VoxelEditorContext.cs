using SemagGames.SuperVoxelEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public readonly struct VoxelEditorContext
    {
        public readonly VoxelVolume Volume;
        public readonly Vector3 VoxelPosition;
        public readonly BuildTool SelectedBuildTool;
        
        public readonly bool ValidVoxelPosition;

        public VoxelEditorContext(VoxelVolume volume, Vector3 voxelPosition, BuildTool selectedBuildTool, bool validVoxelPosition = true)
        {
            Volume = volume;
            VoxelPosition = voxelPosition;
            SelectedBuildTool = selectedBuildTool;
            ValidVoxelPosition = validVoxelPosition;
        }
    }
}