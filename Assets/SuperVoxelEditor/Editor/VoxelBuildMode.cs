using System.Collections.Generic;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class VoxelBuildMode : BuildMode
    {
        private readonly HashSet<Vector3> placedVoxels = new HashSet<Vector3>();

        private bool isMouseDown;
        
        public override void HandleMouseDown(VoxelVolumeEditor editor)
        {
            isMouseDown = true;
        }

        public override void HandleMouseUp(VoxelVolumeEditor editor)
        {
            isMouseDown = false;
            placedVoxels.Clear();
        }

        public override void OnUpdate(VoxelVolumeEditor editor)
        {
            if (isMouseDown && !placedVoxels.Contains(editor.VoxelPosition))
            {
                int size = editor.Inspector.VoxelSize;
                
                if (size == 1)
                {
                    editor.Volume.SetVoxel(editor.VoxelPosition, editor.Volume.ColorPicker.SelectedColorIndex, editor.Volume.VoxelProperty.ID);
                    placedVoxels.Add(editor.VoxelPosition);
                }
                else
                {
                    if (editor.Inspector.SelectedShape == Shapes.Cube)
                    {
                        DrawCube(editor, size);
                    }
                    else if (editor.Inspector.SelectedShape == Shapes.Sphere)
                    {
                        DrawSphere(editor, size);
                    }
                }
            }
        }

        private void DrawCube(VoxelVolumeEditor editor, int size)
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        Vector3 voxelPosition = editor.VoxelPosition + new Vector3(x, y, z);

                        editor.Volume.SetVoxel(voxelPosition, editor.Volume.ColorPicker.SelectedColorIndex, editor.Volume.VoxelProperty.ID);
                        placedVoxels.Add(voxelPosition);
                    }
                }
            }
        }
        
        private void DrawSphere(VoxelVolumeEditor editor, int size)
        {
            Vector3Int voxelPosition = Vector3Int.FloorToInt(editor.VoxelPosition);

            for (int x = -size; x <= size; x++)
            {
                for (int y = -size; y <= size; y++)
                {
                    for (int z = -size; z <= size; z++)
                    {
                        Vector3Int offset = new Vector3Int(x, y, z);
                        Vector3Int position = voxelPosition + offset;

                        if (Vector3Int.Distance(voxelPosition, position) < size)
                        {
                            editor.Volume.SetVoxel(position, editor.Volume.ColorPicker.SelectedColorIndex, editor.Volume.VoxelProperty.ID);
                            placedVoxels.Add(position);
                        }
                    }
                }
            }
        }
    }
}