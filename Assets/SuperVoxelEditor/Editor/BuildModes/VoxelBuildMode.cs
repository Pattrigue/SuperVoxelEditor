using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor.BuildModes
{
    public sealed class VoxelBuildMode : VoxelVolumeBuildMode
    {
        public override BuildMode BuildMode => BuildMode.Voxel;

        public Shape SelectedShape { get; set; } = Shape.Cube;
        
        public int VoxelSize { get; set; } = 1;
        
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
            
            if (VoxelSize == 1)
            {
                editor.SetVoxel(editor.VoxelPosition);
                placedVoxels.Add(editor.VoxelPosition);
            }
            else
            {
                Vector3[] worldPositions = SelectedShape switch
                {
                    Shape.Cube => DrawCube(editor, VoxelSize),
                    Shape.Sphere => DrawSphere(editor, VoxelSize),
                    _ => throw new System.NotImplementedException()
                };

                editor.SetVoxels(worldPositions);
            }
            
            lastVoxelPosition = editor.VoxelPosition;
        }

        private void DrawPreview(VoxelVolumeEditor editor)
        {
            Color handlesColor = Handles.color;
            
            if (SelectedShape == Shape.Sphere)
            { 
                Camera sceneCamera = SceneView.lastActiveSceneView.camera;
                
                Handles.color = Color.cyan;
                Handles.DrawWireDisc(editor.VoxelPosition, sceneCamera.transform.forward, VoxelSize, 5f);
                Handles.color = new Color(0, 0.8f, 0.8f, 0.2f);
                Handles.DrawSolidDisc(editor.VoxelPosition, sceneCamera.transform.forward, VoxelSize);
            }
            else if (SelectedShape == Shape.Cube)
            {
                Vector3 offset = VoxelSize % 2 == 0 ? Vector3.one * 0.5f : Vector3.zero;
                Vector3 center = editor.VoxelPosition + offset;

                Handles.color = Color.cyan;
                Handles.DrawWireCube(center, Vector3.one * VoxelSize);
                Handles.color = new Color(0, 0.8f, 0.8f, 0.2f);
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                Handles.CubeHandleCap(0, center, Quaternion.identity, VoxelSize, EventType.Repaint);
            }
            
            Handles.color = handlesColor;
        }

        private Vector3[] DrawCube(VoxelVolumeEditor editor, int size)
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

            return worldPositions.ToArray();
        }
        
        private Vector3[] DrawSphere(VoxelVolumeEditor editor, int size)
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

            return worldPositions.ToArray();
        }
    }
}