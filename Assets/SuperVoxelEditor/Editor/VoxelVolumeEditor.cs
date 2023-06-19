using SemagGames.SuperVoxelEditor;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    [CustomEditor(typeof(VoxelVolume))]
    public sealed class VoxelVolumeEditor : UnityEditor.Editor
    {
        private VoxelVolume Volume => (VoxelVolume)target;

        private GameObject previewCube;
        private Renderer previewCubeRenderer;
        private Material previewCubeMaterial;

        private Vector3 clickedVoxelPosition;

        private float controlledVoxelDistance = 10f;

        private bool isEditingActive = true;
        private bool isDragging;
        private bool foldout;
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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("voxelProperty"));

            if (Volume.VoxelProperty == null)
            {
                EditorGUILayout.HelpBox("You have not assigned a Voxel Property - placing a voxel will default to placing Air voxels!", MessageType.Warning);
            }
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("colorPicker"));

            if (!isEditingActive && GUILayout.Button("Edit"))
            {
                isEditingActive = true;
            }
            else if (isEditingActive && GUILayout.Button("Stop Editing"))
            {
                isEditingActive = false;
            }

            if (GUILayout.Button("Clear Volume") && EditorUtility.DisplayDialog("Clear Volume", "Are you sure you want to clear the volume?", "Yes", "No"))
            {
                Volume.Clear();
            }

            foldout = EditorGUILayout.Foldout(foldout, "References");
            
            if (foldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("chunkPrefab"));
            }

            serializedObject.ApplyModifiedProperties(); // Apply changes after all fields have been drawn
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            GameObject selectedGameObject = Selection.activeGameObject;

            if (!isEditingActive) return;
            if (!IsValidSelection(selectedGameObject)) return;

            Tools.current = Tool.None;
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
            return selectedGameObject != null && selectedGameObject.TryGetComponent(out VoxelVolume _);
        }

        private void HandleSceneGUIEvents(Event currentEvent, SceneView sceneView)
        {
            // Update the delete mode based on whether the Shift key is held down.
            deleteMode = currentEvent.shift;

            // Get the voxel hit point using either control or raycast method.
            bool validVoxelPosition = CalculateVoxelPosition(currentEvent, out Vector3 voxelPosition);

            // Handle mouse click events.
            if (validVoxelPosition)
            {
                HandleMouseClickEvents(currentEvent, voxelPosition);
            }

            // Update the preview cube.
            UpdatePreviewCube(voxelPosition, validVoxelPosition);

            sceneView.Repaint();
        }
        
        private bool CalculateVoxelPosition(Event currentEvent, out Vector3 voxelPosition)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);

            if (currentEvent.control)
            {
                voxelPosition = CalculateControlledVoxelPosition(ray);
                return true;
            }

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                voxelPosition = CalculateRaycastVoxelPosition(hit);
                return true;
            }
            
            // If no collision, set the position to where the ray would intersect with the y=0 plane
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

            if (!deleteMode)
            {
                position += hit.normal;
            }

            return position;
        }
        
        private void HandleMouseClickEvents(Event currentEvent, Vector3 voxelPosition)
        {
            if (currentEvent.button == 0 && voxelPosition != Vector3.zero)
            { 
                if (currentEvent.type == EventType.MouseDown)
                {
                    HandleMouseDownEvent(voxelPosition);
                }
                else if (currentEvent.type == EventType.MouseUp && isDragging)
                {
                    HandleMouseUpEvent(voxelPosition);
                }
            }
        }

        private void HandleMouseDownEvent(Vector3 voxelPosition)
        {
            clickedVoxelPosition = voxelPosition;
            isDragging = true;
        }

        private void HandleMouseUpEvent(Vector3 voxelPosition)
        {
            isDragging = false;
    
            Vector3Int start = Vector3Int.FloorToInt(clickedVoxelPosition);
            Vector3Int end = Vector3Int.FloorToInt(voxelPosition);
    
            Vector3Int min = Vector3Int.Min(start, end);
            Vector3Int max = Vector3Int.Max(start, end);

            uint voxelPropertyId = 0;

            if (Volume.VoxelProperty != null && !deleteMode)
            {
                voxelPropertyId = Volume.VoxelProperty.ID;
            }

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    for (int z = min.z; z <= max.z; z++)
                    {
                        Volume.SetVoxel(new Vector3(x, y, z), Volume.ColorPicker.SelectedColorIndex, voxelPropertyId);
                    }
                }
            }
        }

        private void UpdatePreviewCube(Vector3 voxelPosition, bool hit)
        {
            const float offset = 1.01f;

            if (!hit)
            {
                previewCubeRenderer.enabled = false;
                return;
            }
            
            previewCubeRenderer.enabled = true;
            
            if (isDragging)
            {
                previewCube.transform.position = (voxelPosition + clickedVoxelPosition) * 0.5f;
                previewCube.transform.localScale = new Vector3(
                    Mathf.Abs(voxelPosition.x - clickedVoxelPosition.x) + offset,
                    Mathf.Abs(voxelPosition.y - clickedVoxelPosition.y) + offset,
                    Mathf.Abs(voxelPosition.z - clickedVoxelPosition.z) + offset
                );
            }
            else
            {
                Vector3 cubeSize = new(offset, offset, offset);
                previewCube.transform.position = voxelPosition;
                previewCube.transform.localScale = cubeSize;
                previewCubeMaterial.color = deleteMode ? new Color(1, 0, 0, 0.25f) : Volume.ColorPicker.SelectedColor;
            }

            Color originalColor = Handles.color;
            
            Handles.color = deleteMode ? Color.red : Color.cyan;
            Handles.DrawWireCube(previewCube.transform.position, previewCube.transform.localScale);
            Handles.color = originalColor;
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