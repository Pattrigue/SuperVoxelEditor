using System;
using System.Collections.Generic;
using SemagGames.SuperVoxelEditor;
using SuperVoxelEditor.Editor.BuildTools;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor.BuildModes
{
    public sealed class FaceBuildMode : VoxelVolumeBuildMode
    {
        public override BuildMode BuildMode => BuildMode.Face;
        
        private enum Axis { X, Y, Z }

        public int MaxExploreLimit { get; set; } = 128;
        
        public override void HandleMouseDown(VoxelVolumeEditor editor)
        {
            if (!editor.Raycaster.TryGetRaycastHit(out RaycastHit hit)) return;

            Vector3 voxelPosition = GetVoxelPosition(editor, hit);
            Vector3 normal = hit.normal;

            Vector3[] neighborDirections = GetNeighborDirections(normal);
            HashSet<Vector3> exploredPositions = ExploreNeighbors(editor, voxelPosition, neighborDirections, MaxExploreLimit);

            Vector3[] worldPositions = CreateWorldPositions(editor, exploredPositions, normal);
            editor.SetVoxels(worldPositions);
        }

        private static Vector3 GetVoxelPosition(VoxelVolumeEditor editor, RaycastHit hit)
        {
            Vector3 voxelPosition = editor.VoxelPosition;
            
            if (editor.BuildTools.SelectedTool == BuildTool.Attach)
            {
                voxelPosition = (hit.point - hit.normal * 0.1f).SnapToVoxelGrid();
            }
            
            return voxelPosition;
        }

        public override void HandleMouseUp(VoxelVolumeEditor editor) { }

        public override void OnUpdate(VoxelVolumeEditor editor)
        {
            if (!editor.Raycaster.TryGetRaycastHit(out RaycastHit hit)) return;
            
            Axis dominantAxis = GetDominantAxis(hit.normal);

            Color color;

            Vector3 center = editor.VoxelPosition + hit.normal * 0.5f;

            if (editor.BuildTools.SelectedTool == BuildTool.Attach)
            {
                center -= hit.normal;
            }
            
            Vector3 size;

            if (dominantAxis == Axis.X)
            {
                color = Color.red;
                size = new Vector3(0.01f, 1, 1);
            }
            else if (dominantAxis == Axis.Y)
            {
                size = new Vector3(1, 0.01f, 1);
                color = Color.yellow;
            }
            else
            {
                color = Color.blue;
                size = new Vector3(1, 1, 0.01f);
            }
            
            Handles.color = color;
            Handles.DrawWireCube(center, size);
        }
        
        private static Vector3[] GetNeighborDirections(Vector3 normal)
        {
            Axis dominantAxis = GetDominantAxis(normal);

            Vector3[] neighborDirections = dominantAxis switch
            {
                Axis.X => new[] { Vector3.up, Vector3.down, Vector3.forward, Vector3.back },
                Axis.Y => new[] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back },
                _ => new[] { Vector3.up, Vector3.down, Vector3.left, Vector3.right }
            };

            return neighborDirections;
        }

        private static HashSet<Vector3> ExploreNeighbors(VoxelVolumeEditor editor, Vector3 voxelPosition, Vector3[] neighborDirections, int maxExploreLimit)
        {
            Voxel baseVoxel = editor.Volume.GetVoxel(voxelPosition);
            
            HashSet<Vector3> exploredPositions = new HashSet<Vector3>();

            Queue<Vector3> toExplore = new Queue<Vector3>();
            toExplore.Enqueue(voxelPosition);

            while (toExplore.TryDequeue(out Vector3 position))
            {
                if (exploredPositions.Count > maxExploreLimit) break;
                
                foreach (Vector3 neighborDirection in neighborDirections)
                {
                    Vector3 neighborPosition = position + neighborDirection;

                    if (exploredPositions.Contains(neighborPosition)) continue;
                    if (!editor.Volume.TryGetVoxel(neighborPosition, out Voxel voxel)) continue;
                    if (voxel != baseVoxel) continue;

                    toExplore.Enqueue(neighborPosition);
                }

                exploredPositions.Add(position);
            }

            return exploredPositions;
        }

        private static Axis GetDominantAxis(Vector3 normal)
        {
            Vector3 absNormal = new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z));

            if (absNormal.x > absNormal.y && absNormal.x > absNormal.z)
            {
                return Axis.X;
            }

            if (absNormal.y >= absNormal.x && absNormal.y >= absNormal.z)
            {
                return Axis.Y;
            }

            return Axis.Z;
        }
        
        private static Vector3[] CreateWorldPositions(VoxelVolumeEditor editor, HashSet<Vector3> exploredPositions, Vector3 normal)
        {
            Vector3[] worldPositions = new Vector3[exploredPositions.Count];
            
            int i = 0;

            foreach (Vector3 exploredPosition in exploredPositions)
            {
                if (editor.BuildTools.SelectedTool == BuildTool.Attach)
                {
                    Vector3 extrudedPosition = exploredPosition + normal;

                    if (!editor.Volume.HasVoxel(extrudedPosition))
                    {
                        worldPositions[i] = extrudedPosition;
                    }
                }
                else
                {
                    worldPositions[i] = exploredPosition;
                }

                i++;
            }

            Array.Resize(ref worldPositions, i);
            
            return worldPositions;
        }
    }
}