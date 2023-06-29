using SemagGames.SuperVoxelEditor;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class SceneGuiEventProcessor
    {
        private readonly VoxelVolumeEditor editor;
        private readonly VoxelVolumeEventProcessor eventProcessor;
        private readonly ChunkBoundsDrawer chunkBoundsDrawer;
        
        private float controlledVoxelDistance = 10f;
    
        public SceneGuiEventProcessor(VoxelVolumeEditor editor)
        {
            this.editor = editor;
            chunkBoundsDrawer = new ChunkBoundsDrawer(editor.Volume.Chunks, Color.red);
            eventProcessor = new VoxelVolumeEventProcessor(editor);
        }

        public void ProcessSceneGUIEvents(SceneView sceneView)
        {
            eventProcessor.ProcessKeyPressEvents();
        
            GameObject selectedGameObject = Selection.activeGameObject;

            if (!editor.Inspector.IsEditingActive) return;
            if (!IsValidSelection(selectedGameObject)) return;

            Tools.current = Tool.None;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            HandleSceneGuiEvents(sceneView);
        
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

        private void HandleSceneGuiEvents(SceneView sceneView)
        {
            // Get the voxel hit point using either control or raycast method
            if (Event.current.control)
            {
                editor.Raycaster.CalculateControlledVoxelPosition(controlledVoxelDistance);
            }
            else
            {
                editor.Raycaster.CalculateVoxelPosition();
            }

            // Handle mouse click events.
            eventProcessor.ProcessMouseClickEvents();

            // Update build mode
            editor.ActiveBuildMode.OnUpdate(editor);

            if (editor.Inspector.DrawChunkBounds)
            {
                chunkBoundsDrawer.Draw();
            }

            sceneView.Repaint();
        }
    }
}