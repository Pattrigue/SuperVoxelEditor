using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class BoxBuildMode : BuildMode
    {
        private readonly PreviewCube previewCube;
        
        private Vector3 mouseDownVoxelPosition;
        
        private bool isDragging;

        public BoxBuildMode()
        {
            previewCube = new PreviewCube();
        }
        
        public override void HandleMouseDown(VoxelEditorContext ctx)
        {
            mouseDownVoxelPosition = ctx.VoxelPosition;
            isDragging = true;
        }

        public override void HandleMouseUp(VoxelEditorContext ctx)
        {
            if (isDragging)
            {
                if (ctx.ValidVoxelPosition)
                {
                    PlaceVoxels(ctx);
                }
                else
                {
                    isDragging = false;
                }
            }
        }

        public override void UpdatePreview(VoxelEditorContext ctx)
        {
            previewCube.Update(ctx.VoxelPosition, mouseDownVoxelPosition, ctx.Volume.ColorPicker.SelectedColor, ctx.ValidVoxelPosition, isDragging, ctx.SelectedBuildTool);
        }

        private void PlaceVoxels(VoxelEditorContext e)
        {
            isDragging = false;

            Vector3Int start = Vector3Int.FloorToInt(mouseDownVoxelPosition);
            Vector3Int end = Vector3Int.FloorToInt(e.VoxelPosition);

            Vector3Int min = Vector3Int.Min(start, end);
            Vector3Int max = Vector3Int.Max(start, end);
            
            int sizeX = max.x - min.x + 1;
            int sizeY = max.y - min.y + 1;
            int sizeZ = max.z - min.z + 1;

            Vector3[] worldPositions = new Vector3[sizeX * sizeY * sizeZ];
            
            uint voxelPropertyId = 0;

            if (e.Volume.VoxelProperty != null && e.SelectedBuildTool is not BuildTool.Erase)
            {
                voxelPropertyId = e.Volume.VoxelProperty.ID;
            }

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

            if (worldPositions.Length == 1)
            {
                e.Volume.SetVoxel(worldPositions[0], e.Volume.ColorPicker.SelectedColorIndex, voxelPropertyId);
            }
            else
            {
                e.Volume.SetVoxels(worldPositions, e.Volume.ColorPicker.SelectedColorIndex, voxelPropertyId);
            }
        }
    }
}