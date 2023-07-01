using SemagGames.SuperVoxelEditor;
using SuperVoxelEditor.Editor.BuildModes;
using SuperVoxelEditor.Editor.BuildTools;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class SceneVoxelRaycaster
    {
        public Vector3 VoxelPosition => voxelPosition;

        public bool IsValidVoxelPosition { get; private set; }
        
        private readonly VoxelVolumeEditor editor;

        private Vector3 voxelPosition;
        private RaycastHit raycastHit;

        private bool didRaycastHit;

        public SceneVoxelRaycaster(VoxelVolumeEditor editor)
        {
            this.editor = editor;
        }

        public bool TryGetRaycastHit(out RaycastHit hit)
        {
            if (didRaycastHit)
            {
                hit = raycastHit;
                return true;
            }

            hit = default;
            
            return false;
        }
        
        public void CalculateVoxelPosition()
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            didRaycastHit = Physics.Raycast(ray, out raycastHit, Mathf.Infinity); 
            
            if (didRaycastHit)
            {
                CalculateRaycastVoxelPosition(raycastHit);
                IsValidVoxelPosition = true;
                return;
            }
            
            // If the ray did not hit any collider, set the position to where the ray would intersect with the y=0 plane
            float distanceToYZeroPlane = -ray.origin.y / ray.direction.y;
            
            if (distanceToYZeroPlane >= 0) // Check to prevent intersecting the y=0 plane behind the origin
            {
                voxelPosition = ray.origin + ray.direction * distanceToYZeroPlane;
                voxelPosition.y = 0;
                voxelPosition = voxelPosition.SnapToVoxelGrid();
                
                IsValidVoxelPosition = true;
                
                return;
            }
            
            // Default to Vector3.zero if ray is parallel to y=0 plane
            voxelPosition = Vector3.zero;
            IsValidVoxelPosition = false;
        }
        
        public void CalculateControlledVoxelPosition(float controlledVoxelDistance)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            voxelPosition = (ray.origin + ray.direction * controlledVoxelDistance).SnapToVoxelGrid();
            IsValidVoxelPosition = true;
        }

        private void CalculateRaycastVoxelPosition(RaycastHit hit)
        {
            Vector3 position = hit.point - hit.normal * 0.1f;

            if (editor.BuildTools.Inspector.SelectedTool == BuildTool.Attach)
            {
                if (editor.ActiveBuildMode is VoxelBuildMode voxelBuildMode && voxelBuildMode.VoxelSize > 1)
                {
                    Vector3 offset = new Vector3(
                        hit.normal.x < 0 ? -0.5f : 0f,
                        hit.normal.y < 0 ? -0.5f : 0f,
                        hit.normal.z < 0 ? -0.5f : 0f
                    );
                    position += hit.normal * (voxelBuildMode.VoxelSize * 0.5f) + offset;
                }
                else
                {
                    position += hit.normal;
                }
            }

            voxelPosition = position.SnapToVoxelGrid();
        }
    }
}