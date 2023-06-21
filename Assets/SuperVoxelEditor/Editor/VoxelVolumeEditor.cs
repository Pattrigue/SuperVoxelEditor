using SemagGames.SuperVoxelEditor;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    [CustomEditor(typeof(VoxelVolume))]
    public sealed class VoxelVolumeEditor : UnityEditor.Editor
    {
        private VoxelVolume Volume => (VoxelVolume)target;

        private PreviewCube previewCube;
        private BuildTools buildTools;
        private VoxelVolumeInspectorDrawer inspectorDrawer;
        private BuildMode buildMode;
        
        private VoxelEditorContext currentVoxelEditorContext;
        
        private float controlledVoxelDistance = 10f;

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            
            previewCube ??= new PreviewCube();
            buildTools ??= new BuildTools();
            inspectorDrawer ??= new VoxelVolumeInspectorDrawer();
            buildMode ??= new BoxBuildMode();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            
            previewCube?.Destroy();
            buildTools = null;
            inspectorDrawer = null;
            buildMode = null;
            previewCube = null;
        }

        public override void OnInspectorGUI()
        {
            inspectorDrawer.DrawInspectorGUI(this, serializedObject, buildTools.DrawInspectorGUI);
        }
        
        private void OnSceneGUI(SceneView sceneView)
        {
            HandleKeyPressEvents();
            
            GameObject selectedGameObject = Selection.activeGameObject;

            if (!inspectorDrawer.IsEditingActive) return;
            if (!IsValidSelection(selectedGameObject)) return;

            Tools.current = Tool.None;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    
            HandleSceneGUIEvents(sceneView);
            
            if (Event.current.control && Event.current.type == EventType.ScrollWheel)
            {
                controlledVoxelDistance -= Event.current.delta.y;
                controlledVoxelDistance = Mathf.Clamp(controlledVoxelDistance, 1f, 100f);
                Event.current.Use();  // Prevents the event from propagating further.
            }
        }
      
        private void HandleKeyPressEvents()
        {
            Event e = Event.current;

            if (e.type != EventType.KeyDown) return;

            if (e.control)
            {
                if (Event.current.keyCode == KeyCode.Z)
                {
                    Volume.Undo();
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Y)
                {
                    Volume.Redo();
                    e.Use();
                }
            }

            buildTools.HandleKeyPressEvents(e);
        }
        
        private static bool IsValidSelection(GameObject selectedGameObject)
        {
            return selectedGameObject != null && selectedGameObject.TryGetComponent(out VoxelVolume _);
        }

        private void HandleSceneGUIEvents(SceneView sceneView)
        {
            // Get the voxel hit point using either control or raycast method.
            bool validVoxelPosition = CalculateVoxelPosition(out Vector3 voxelPosition);
            currentVoxelEditorContext = new VoxelEditorContext(Volume, voxelPosition, buildTools.SelectedTool, validVoxelPosition);

            // Handle mouse click events.
            HandleMouseClickEvents();

            // Update the preview cube.
            buildMode.UpdatePreview(currentVoxelEditorContext);

            if (inspectorDrawer.DrawChunkBounds)
            {
                DrawChunkBounds();
            }

            sceneView.Repaint();
        }

        private bool CalculateVoxelPosition(out Vector3 voxelPosition)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            if (Event.current.control)
            {
                voxelPosition = CalculateControlledVoxelPosition(ray);
                return true;
            }

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                voxelPosition = CalculateRaycastVoxelPosition(hit);
                return true;
            }
            
            // If the ray did not hit any collider, set the position to where the ray would intersect with the y=0 plane
            float distanceToYZeroPlane = -ray.origin.y / ray.direction.y;
            
            if (distanceToYZeroPlane >= 0) // Check to prevent intersecting the y=0 plane behind the origin
            {
                voxelPosition = ray.origin + ray.direction * distanceToYZeroPlane;
                voxelPosition.y = 0;
                
                SnapToVoxelGrid(ref voxelPosition);
            
                return true;
            }
            
            // Default to Vector3.zero if ray is parallel to y=0 plane
            voxelPosition = Vector3.zero;
            return false;
        }

        private Vector3 CalculateControlledVoxelPosition(Ray ray)
        {
            Vector3 voxelPosition = ray.origin + ray.direction * controlledVoxelDistance;
            SnapToVoxelGrid(ref voxelPosition);
            
            return voxelPosition;
        }

        private Vector3 CalculateRaycastVoxelPosition(RaycastHit hit)
        {
            Vector3 position = hit.point - hit.normal * 0.1f;
            SnapToVoxelGrid(ref position);

            if (buildTools.SelectedTool == BuildTool.Attach)
            {
                position += hit.normal;
            }

            return position;
        }

        private void HandleMouseClickEvents()
        {
            if (Event.current.button != 0) return;

            if (Event.current.type == EventType.MouseDown && currentVoxelEditorContext.ValidVoxelPosition)
            {
                if (buildTools.SelectedTool is BuildTool.Picker)
                {
                    buildTools.PickVoxelAtPosition(currentVoxelEditorContext.Volume, currentVoxelEditorContext.VoxelPosition);
                    return;
                }

                buildMode.HandleMouseDown(currentVoxelEditorContext);
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                buildMode.HandleMouseUp(currentVoxelEditorContext);
            }
        }

        private void DrawChunkBounds()
        {
            foreach (Chunk chunk in Volume.Chunks)
            {
                Vector3 size = new Vector3(Chunk.Width, Chunk.Height, Chunk.Width);
                Vector3 center = chunk.transform.position + size * 0.5f;

                Color handlesColor = Handles.color;

                Handles.color = Color.red;
                Handles.DrawWireCube(center, size);
                Handles.color = handlesColor;
            }
        }
        
        private static void SnapToVoxelGrid(ref Vector3 position)
        {
            position = new Vector3(
                Mathf.FloorToInt(position.x) + 0.5f,
                Mathf.FloorToInt(position.y) + 0.5f,
                Mathf.FloorToInt(position.z) + 0.5f
            );
        }
    }
}