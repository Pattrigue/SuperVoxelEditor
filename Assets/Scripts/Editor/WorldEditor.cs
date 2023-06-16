using UnityEditor;
using UnityEngine;

namespace SemagGames.VoxelEditor.Editor
{
    [CustomEditor(typeof(World))]
    public sealed class WorldEditor : UnityEditor.Editor
    {
        private World World => (World)target;

        private GameObject previewCube;
        private Renderer previewCubeRenderer;
        private Material previewCubeMaterial;

        private Vector3 clickedVoxelPosition;

        private float controlledVoxelDistance = 10f;

        private bool isDragging;
        private bool deleteMode;

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;

            if (previewCube == null)
            {
                previewCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                previewCube.hideFlags = HideFlags.HideAndDontSave;
            
                previewCubeMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                previewCubeRenderer = previewCube.GetComponent<Renderer>();
                previewCubeRenderer.material = previewCubeMaterial;
            
                DestroyImmediate(previewCube.GetComponent<Collider>());
            }
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;

            if (previewCube != null)
            {
                DestroyImmediate(previewCubeMaterial);
                DestroyImmediate(previewCube);
                previewCube = null;
            }
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Clear"))
            {
                World.Clear();
            }
        
            DrawDefaultInspector();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            GameObject selectedGameObject = Selection.activeGameObject;

            if (!IsValidSelection(selectedGameObject)) return;

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    
            HandleSceneGUIEvents(Event.current, sceneView);
            
            if (Event.current.control && Event.current.type == EventType.ScrollWheel)
            {
                controlledVoxelDistance -= Event.current.delta.y;
                controlledVoxelDistance = Mathf.Clamp(controlledVoxelDistance, 1f, 100f);
                Event.current.Use();  // Prevents the event from propagating further.
            }
        }
        
        private static bool IsValidSelection(GameObject selectedGameObject)
        {
            return selectedGameObject != null && selectedGameObject.TryGetComponent(out World _);
        }

        private void HandleSceneGUIEvents(Event currentEvent, SceneView sceneView)
        {
            // Update the delete mode based on whether the Shift key is held down.
            deleteMode = currentEvent.shift;

            // Get the voxel hit point using either control or raycast method.
            bool hitVoxel = CalculateVoxelHitPoint(currentEvent, out Vector3 voxelHitPoint);

            // Handle mouse click events.
            if (hitVoxel)
            {
                HandleMouseClickEvents(currentEvent, voxelHitPoint);
            }

            // Update the preview cube.
            UpdatePreviewCube(voxelHitPoint, hitVoxel);

            sceneView.Repaint();
        }
        
        private bool CalculateVoxelHitPoint(Event currentEvent, out Vector3 voxelHitPoint)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);

            if (currentEvent.control)
            {
                voxelHitPoint = CalculateControlledVoxelHitPoint(ray);
                return true;
            }

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                voxelHitPoint = CalculateRaycastVoxelHitPoint(hit);
                return true;
            }
            
            voxelHitPoint = Vector3.zero;

            return false;
        }

        private Vector3 CalculateControlledVoxelHitPoint(Ray ray)
        {
            Vector3 voxelHitPoint = ray.origin + ray.direction * controlledVoxelDistance;
            SnapToVoxelGrid(ref voxelHitPoint);
            
            return voxelHitPoint;
        }

        private Vector3 CalculateRaycastVoxelHitPoint(RaycastHit hit)
        {
            Vector3 hitPoint = hit.point - hit.normal * 0.1f;
            SnapToVoxelGrid(ref hitPoint);

            if (!deleteMode)
            {
                hitPoint += hit.normal;
            }

            return hitPoint;
        }
        
        private void HandleMouseClickEvents(Event currentEvent, Vector3 voxelHitPoint)
        {
            if (currentEvent.button == 0 && voxelHitPoint != Vector3.zero)
            { 
                if (currentEvent.type == EventType.MouseDown)
                {
                    HandleMouseDownEvent(voxelHitPoint);
                }
                else if (currentEvent.type == EventType.MouseUp && isDragging)
                {
                    HandleMouseUpEvent(voxelHitPoint);
                }
            }
        }

        private void HandleMouseDownEvent(Vector3 voxelHitPoint)
        {
            clickedVoxelPosition = voxelHitPoint;
            isDragging = true;
        }

        private void HandleMouseUpEvent(Vector3 voxelHitPoint)
        {
            isDragging = false;
    
            Vector3Int start = Vector3Int.FloorToInt(clickedVoxelPosition);
            Vector3Int end = Vector3Int.FloorToInt(voxelHitPoint);
    
            Vector3Int min = Vector3Int.Min(start, end);
            Vector3Int max = Vector3Int.Max(start, end);

            uint voxelId = deleteMode ? Voxel.AirId : 1u;

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    for (int z = min.z; z <= max.z; z++)
                    {                
                        World.SetVoxel(new Vector3(x, y, z), voxelId);
                    }
                }
            }
        }

        private void UpdatePreviewCube(Vector3 voxelHitPoint, bool hitVoxel)
        {
            const float offset = 1.01f;

            if (!hitVoxel)
            {
                previewCubeRenderer.enabled = false;
                return;
            }
        
            previewCubeRenderer.enabled = true;
            
            if (isDragging)
            {
                previewCube.transform.position = (voxelHitPoint + clickedVoxelPosition) * 0.5f;
                previewCube.transform.localScale = new Vector3(
                    Mathf.Abs(voxelHitPoint.x - clickedVoxelPosition.x) + offset,
                    Mathf.Abs(voxelHitPoint.y - clickedVoxelPosition.y) + offset,
                    Mathf.Abs(voxelHitPoint.z - clickedVoxelPosition.z) + offset
                );
            }
            else
            {
                Vector3 cubeSize = new(offset, offset, offset);
                previewCube.transform.position = voxelHitPoint;
                previewCube.transform.localScale = cubeSize;
                previewCubeMaterial.color = deleteMode ? new Color(1, 0, 0, 0.25f) : World.ColorPicker.SelectedColor;
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