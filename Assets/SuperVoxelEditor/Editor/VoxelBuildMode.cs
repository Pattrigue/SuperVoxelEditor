using System.Collections.Generic;
using UnityEditor;
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
            DrawPreview(editor);
            
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
                if (editor.Inspector.SelectedShape == Shape.Cube)
                {
                    DrawCube(editor, size);
                }
                else if (editor.Inspector.SelectedShape == Shape.Sphere)
                {
                    DrawSphere(editor, size);
                }
            }
            
            lastVoxelPosition = editor.VoxelPosition;
        }

        private static void DrawPreview(VoxelVolumeEditor editor)
        {
            Color handlesColor = Handles.color;
            
            if (editor.Inspector.SelectedShape == Shape.Sphere)
            { 
                Camera sceneCamera = SceneView.lastActiveSceneView.camera;
                
                Handles.color = Color.cyan;
                Handles.DrawWireDisc(editor.VoxelPosition, sceneCamera.transform.forward, editor.Inspector.VoxelSize, 5f);
                Handles.color = new Color(0, 0.8f, 0.8f, 0.2f);
                Handles.DrawSolidDisc(editor.VoxelPosition, sceneCamera.transform.forward, editor.Inspector.VoxelSize);
            }
            else if (editor.Inspector.SelectedShape == Shape.Cube)
            {
                int size = editor.Inspector.VoxelSize;
                
                Vector3 offset = size % 2 == 0 ? Vector3.one * 0.5f : Vector3.zero;
                Vector3 center = editor.VoxelPosition + offset;

                Handles.color = Color.cyan;
                Handles.DrawWireCube(center, Vector3.one * size);
                Handles.color = new Color(0, 0.8f, 0.8f, 0.2f);
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                Handles.CubeHandleCap(0, center, Quaternion.identity, size, EventType.Repaint);
            }
            
            Handles.color = handlesColor;
        }

        private void DrawCube(VoxelVolumeEditor editor, int size)
        {
            List<Vector3> worldPositions = new List<Vector3>();
            Vector3 offset = size % 2 == 0 ? Vector3.one * 0.5f : Vector3.zero;
            Vector3 center = editor.VoxelPosition + offset;

            if (size % 2 == 0)
            {
                // Draw a cube with an even size
                for (int x = -size / 2; x < size / 2; x++)
                {
                    for (int y = -size / 2; y < size / 2; y++)
                    {
                        for (int z = -size / 2; z < size / 2; z++)
                        {
                            Vector3Int position = Vector3Int.FloorToInt(center) + new Vector3Int(x, y, z);
                            worldPositions.Add(position);
                            placedVoxels.Add(position);
                        }
                    }
                }
            }
            else
            {
                // Draw a cube with an odd size
                for (int x = -size / 2; x <= size / 2; x++)
                {
                    for (int y = -size / 2; y <= size / 2; y++)
                    {
                        for (int z = -size / 2; z <= size / 2; z++)
                        {
                            Vector3Int position = Vector3Int.FloorToInt(center) + new Vector3Int(x, y, z);
                            worldPositions.Add(position);
                            placedVoxels.Add(position);
                        }
                    }
                }
            }

            editor.Volume.SetVoxels(worldPositions.ToArray(), editor.Volume.ColorPicker.SelectedColorIndex, editor.Volume.VoxelProperty.ID);
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