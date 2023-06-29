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
            if (editor.BuildTools.SelectedTool == BuildTool.Cover 
                && (editor.Volume.HasVoxel(worldPosition + Vector3.up)
                    || !editor.Volume.HasVoxel(worldPosition - Vector3.up)))
            {
                return;
            }
        
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

            if (editor.BuildTools.SelectedTool == BuildTool.Cover)
            {
                worldPositions = worldPositions
                    .Where(position =>
                        editor.Volume.HasVoxel(position)
                        && editor.Volume.HasVoxel(position - Vector3.up)
                        && !editor.Volume.HasVoxel(position + Vector3.up)
                        && editor.Volume.TryGetVoxel(position - Vector3.up, out var voxelBelow)
                        && !voxelBelow.GetColor().IsSameColor(voxelColor)
                    )
                    .ToArray();
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