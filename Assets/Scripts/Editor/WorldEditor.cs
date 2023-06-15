using UnityEditor;
using UnityEngine;

namespace SemagGames.VoxelEditor.Editor
{
    [CustomEditor(typeof(World))]
    public sealed class WorldEditor : UnityEditor.Editor
    {
        private World World => (World)target;

        private GameObject previewCube;
        private Material previewCubeMaterial;

        private Vector3 clickedVoxelPosition;

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
                previewCube.GetComponent<Renderer>().material = previewCubeMaterial;
            
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

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            deleteMode = Event.current.shift;

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                Vector3 voxelHitPoint = CalculateVoxelHitPoint(hit);

                if (Event.current.button == 0)
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        HandleMouseDownEvent(voxelHitPoint);
                    }
                    else if (Event.current.type == EventType.MouseUp && isDragging)
                    {
                        HandleMouseUpEvent(voxelHitPoint);
                    }
                }

                UpdatePreviewCube(voxelHitPoint);
            }
            
            sceneView.Repaint();
        }

        private static bool IsValidSelection(GameObject selectedGameObject)
        {
            return selectedGameObject != null && selectedGameObject.TryGetComponent(out World _);
        }

        private Vector3 CalculateVoxelHitPoint(RaycastHit hit)
        {
            Vector3 hitPoint = hit.point - hit.normal * 0.1f;
        
            Vector3 voxelHitPoint = new(
                Mathf.FloorToInt(hitPoint.x) + 0.5f,
                Mathf.FloorToInt(hitPoint.y) + 0.5f,
                Mathf.FloorToInt(hitPoint.z) + 0.5f
            );

            if (!deleteMode)
            {
                voxelHitPoint += hit.normal;
            }

            return voxelHitPoint;
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

        private void UpdatePreviewCube(Vector3 voxelHitPoint)
        {
            const float offset = 1.01f;
        
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
    }
}