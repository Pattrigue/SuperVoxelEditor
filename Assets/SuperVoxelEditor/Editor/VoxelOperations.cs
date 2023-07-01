using System.Linq;
using SemagGames.SuperVoxelEditor;
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

        public void SetVoxel(Vector3 worldPosition, Color32 voxelColor)
        {
            if (editor.BuildTools.SelectedTool != BuildTool.Erase)
            {
                editor.Volume.SetVoxel(worldPosition, editor.Volume.VoxelProperty.ID, voxelColor);
            }
            else
            {
                editor.Volume.EraseVoxel(worldPosition);
            }
        }
    
        public void SetVoxels(Vector3[] worldPositions, Color32 voxelColor)
        {
            if (worldPositions.Length == 1)
            {
                SetVoxel(worldPositions[0], voxelColor);
                return;
            }

            if (editor.BuildTools.SelectedTool != BuildTool.Erase)
            {
                editor.Volume.SetVoxels(worldPositions, editor.Volume.VoxelProperty.ID, voxelColor);
            }
            else
            {
                editor.Volume.EraseVoxels(worldPositions);
            }
        }
    }
}