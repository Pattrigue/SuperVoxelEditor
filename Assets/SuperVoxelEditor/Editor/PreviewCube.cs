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

        public void Update(VoxelVolumeEditor editor, Vector3 mouseDownVoxelPosition, bool isDragging)
        {
            const float offset = 1.01f;

            if (!editor.ValidVoxelPosition)
            {
                cubeRenderer.enabled = false;
                return;
            }

            bool isErasing = editor.BuildTools.SelectedTool == BuildTool.Erase;

            cubeRenderer.enabled = editor.BuildTools.SelectedTool != BuildTool.Picker;

            if (isDragging)
            {
                cube.transform.position = (editor.VoxelPosition + mouseDownVoxelPosition) * 0.5f;
                cube.transform.localScale = new Vector3(
                    Mathf.Abs(editor.VoxelPosition.x - mouseDownVoxelPosition.x) + offset,
                    Mathf.Abs(editor.VoxelPosition.y - mouseDownVoxelPosition.y) + offset,
                    Mathf.Abs(editor.VoxelPosition.z - mouseDownVoxelPosition.z) + offset
                );
            }
            else
            {
                Vector3 cubeSize = new(offset, offset, offset);
                cube.transform.position = editor.VoxelPosition;
                cube.transform.localScale = cubeSize;
                cubeMaterial.color = isErasing ? new Color(1, 0, 0, 0.25f) : editor.Volume.ColorPicker.SelectedColor;
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