using System.Collections.Generic;
using SemagGames.SuperVoxelEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class VoxelBuildMode : BuildMode
    {
        private readonly HashSet<Vector3> placedVoxels = new HashSet<Vector3>();

        private bool isMouseDown;

        private Vector3 lastVoxelPosition;
        
        public override void HandleMouseDown(VoxelVolumeEditor editor)
        {
            editor.Volume.AutoRebuildChunkColliders = false;
            isMouseDown = true;
        }

        public override void HandleMouseUp(VoxelVolumeEditor editor)
        {
            editor.Volume.AutoRebuildChunkColliders = true;
            isMouseDown = false;
            placedVoxels.Clear();
        }

        public override void OnUpdate(VoxelVolumeEditor editor)
        {
            if (!isMouseDown) return;
            if (placedVoxels.Contains(editor.VoxelPosition)) return;
            if (lastVoxelPosition == editor.VoxelPosition) return;
            
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
            
            lastVoxelPosition = editor.VoxelPosition;
        }

        private void DrawCube(VoxelVolumeEditor editor, int size)
        {
            Vector3[] worldPositions = new Vector3[size * size * size];
            
            int i = 0;
                
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        Vector3 voxelPosition = editor.VoxelPosition + new Vector3(x, y, z);
                        worldPositions[i++] = voxelPosition;
                        placedVoxels.Add(voxelPosition);
                    }
                }
            }
            
            editor.Volume.SetVoxels(worldPositions, editor.Volume.ColorPicker.SelectedColorIndex, editor.Volume.VoxelProperty.ID);
        }
        
        private void DrawSphere(VoxelVolumeEditor editor, int size)
        {
            Vector3Int voxelPosition = Vector3Int.FloorToInt(editor.VoxelPosition);
            List<Vector3> worldPositions = new List<Vector3>(size * size * size);

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
                            worldPositions.Add(position);
                            placedVoxels.Add(position);
                        }
                    }
                }
            }
            
            editor.Volume.SetVoxels(worldPositions.ToArray(), editor.Volume.ColorPicker.SelectedColorIndex, editor.Volume.VoxelProperty.ID);
        }
    }
}