using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class PreviewCube
    {
        private readonly Renderer cubeRenderer;
        private readonly Material cubeMaterial;
        
        private GameObject cube;

        public PreviewCube()
        {
            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.hideFlags = HideFlags.HideAndDontSave;

            cubeMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            cubeRenderer = cube.GetComponent<Renderer>();
            cubeRenderer.material = cubeMaterial;

            Object.DestroyImmediate(cube.GetComponent<Collider>());
        }

        public void Destroy()
        {
            if (cube != null)
            {
                Object.DestroyImmediate(cubeMaterial);
                Object.DestroyImmediate(cube);
                cube = null;
            }
        }

        public void Update(Vector3 voxelPosition, Vector3 mouseDownVoxelPosition, Color32 color, bool validVoxelPosition, bool isDragging, BuildTool selectedTool)
        {
            const float offset = 1.01f;

            if (!validVoxelPosition)
            {
                cubeRenderer.enabled = false;
                return;
            }

            bool isErasing = selectedTool == BuildTool.Erase;

            cubeRenderer.enabled = true;

            if (isDragging)
            {
                cube.transform.position = (voxelPosition + mouseDownVoxelPosition) * 0.5f;
                cube.transform.localScale = new Vector3(
                    Mathf.Abs(voxelPosition.x - mouseDownVoxelPosition.x) + offset,
                    Mathf.Abs(voxelPosition.y - mouseDownVoxelPosition.y) + offset,
                    Mathf.Abs(voxelPosition.z - mouseDownVoxelPosition.z) + offset
                );
            }
            else
            {
                Vector3 cubeSize = new(offset, offset, offset);
                cube.transform.position = voxelPosition;
                cube.transform.localScale = cubeSize;
                cubeMaterial.color = isErasing ? new Color(1, 0, 0, 0.25f) : color;
            }
            
            DrawHandles(isErasing);
        }

        private void DrawHandles(bool deleteMode)
        {
            Color originalColor = Handles.color;

            Handles.color = deleteMode ? Color.red : Color.cyan;
            Handles.DrawWireCube(cube.transform.position, cube.transform.localScale);
            Handles.color = originalColor;
        }
    }
}