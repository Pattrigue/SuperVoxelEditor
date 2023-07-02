using SuperVoxelEditor.Editor.BuildTools;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public sealed class VoxelVolumeEventProcessor
    {
        private readonly VoxelVolumeEditor editor;

        public VoxelVolumeEventProcessor(VoxelVolumeEditor editor)
        {
            this.editor = editor;
        }

        public void ProcessKeyPressEvents()
        {
            Event e = Event.current;

            if (e.type != EventType.KeyDown) return;

            if (e.control)
            {
                if (e.keyCode == KeyCode.Z)
                {
                    editor.Volume.Undo();
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Y)
                {
                    editor.Volume.Redo();
                    e.Use();
                }
            }

            editor.BuildTools.Input.HandleKeyPressEvents(e);
        }

        public void ProcessMouseClickEvents()
        {
            if (Event.current.button != 0) return;

            if (Event.current.type == EventType.MouseDown && editor.Raycaster.IsValidVoxelPosition)
            {
                editor.Volume.Commands.BeginCompositeCommand();
                
                if (editor.BuildTools.Inspector.SelectedTool is BuildTool.Picker)
                {
                    if (VoxelPicker.PickVoxelAtPosition(editor.Volume, editor.VoxelPosition))
                    {
                        editor.BuildTools.Inspector.SelectedTool = BuildTool.Attach;
                    }
                    return;
                }

                editor.ActiveBuildMode.HandleMouseDown(editor);
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                editor.ActiveBuildMode.HandleMouseUp(editor);
                editor.Volume.Commands.EndCompositeCommand();
            }
        }
    }
}