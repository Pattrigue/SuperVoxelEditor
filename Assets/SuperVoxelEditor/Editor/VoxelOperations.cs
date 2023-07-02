using SuperVoxelEditor.Editor.BuildTools;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class VoxelOperations
    {
        private readonly VoxelVolumeEditor editor;
    
        public VoxelOperations(VoxelVolumeEditor editor)
        {
            this.editor = editor;
        }

        public void SetVoxel(Vector3 worldPosition)
        {
            if (editor.BuildTools.SelectedTool != BuildTool.Erase && editor.Volume.VoxelAsset != null)
            {
                editor.Volume.SetVoxel(worldPosition, editor.Volume.VoxelAsset, editor.Inspector.SelectedColor);
            }
            else
            {
                editor.Volume.EraseVoxel(worldPosition);
            }
        }
    
        public void SetVoxels(Vector3[] worldPositions)
        {
            if (worldPositions.Length == 1)
            {
                SetVoxel(worldPositions[0]);
                return;
            }

            if (editor.BuildTools.SelectedTool != BuildTool.Erase && editor.Volume.VoxelAsset != null)
            {
                editor.Volume.SetVoxels(worldPositions, editor.Volume.VoxelAsset, editor.Inspector.SelectedColor);
            }
            else
            {
                editor.Volume.EraseVoxels(worldPositions);
            }
        }
    }
}