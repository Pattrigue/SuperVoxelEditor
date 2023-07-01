﻿using UnityEngine;

namespace SuperVoxelEditor.Editor.BuildModes
{
    public sealed class BoxBuildMode : VoxelVolumeBuildMode
    {
        public override BuildMode BuildMode => BuildMode.Box;
        
        private readonly PreviewCube previewCube;
        
        private Vector3 mouseDownVoxelPosition;
        
        private bool isDragging;

        public BoxBuildMode()
        {
            previewCube = new PreviewCube();
        }
        
        public override void HandleMouseDown(VoxelVolumeEditor editor)
        {
            mouseDownVoxelPosition = editor.VoxelPosition;
            isDragging = true;
        }

        public override void HandleMouseUp(VoxelVolumeEditor editor)
        {
            if (isDragging)
            {
                if (editor.Raycaster.IsValidVoxelPosition)
                {
                    PlaceVoxels(editor);
                }
                else
                {
                    isDragging = false;
                }
            }
        }

        public override void OnUpdate(VoxelVolumeEditor editor)
        {
            previewCube.Update(editor, mouseDownVoxelPosition, isDragging);
        }

        private void PlaceVoxels(VoxelVolumeEditor editor)
        {
            isDragging = false;

            Vector3Int start = Vector3Int.FloorToInt(mouseDownVoxelPosition);
            Vector3Int end = Vector3Int.FloorToInt(editor.VoxelPosition);

            Vector3Int min = Vector3Int.Min(start, end);
            Vector3Int max = Vector3Int.Max(start, end);
            
            int sizeX = max.x - min.x + 1;
            int sizeY = max.y - min.y + 1;
            int sizeZ = max.z - min.z + 1;

            Vector3[] worldPositions = new Vector3[sizeX * sizeY * sizeZ];
            
            int i = 0;

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    for (int z = min.z; z <= max.z; z++)
                    {
                        worldPositions[i++] = new Vector3(x, y, z);
                    }
                }
            }
            
            editor.SetVoxels(worldPositions);
        }
    }
}