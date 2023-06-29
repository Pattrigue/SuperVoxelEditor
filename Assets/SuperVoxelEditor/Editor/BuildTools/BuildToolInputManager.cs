using System;
using UnityEngine;

namespace SuperVoxelEditor.Editor.BuildTools
{
    public sealed class BuildToolInputManager
    {
        private readonly BuildToolInspector buildToolInspector;
 
        public BuildToolInputManager(BuildToolInspector buildToolInspector)
        {
            this.buildToolInspector = buildToolInspector;
        }
 
        public void HandleKeyPressEvents(Event e)
        {
            if (e.shift)
            {
                HandleShiftKeyPressEvents(e.keyCode);
            }
            else
            {
                HandleKeyPressEvents(e.keyCode);
            }
        }
 
        private void HandleKeyPressEvents(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Q:
                    CycleTool(-1);
                    break;
                case KeyCode.E:
                    CycleTool(1);
                    break;
            }
        }
 
        private void HandleShiftKeyPressEvents(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Alpha1:
                    buildToolInspector.SelectedTool = BuildTool.Attach;
                    break;
                case KeyCode.Alpha2:
                    buildToolInspector.SelectedTool = BuildTool.Erase;
                    break;
                case KeyCode.Alpha3:
                    buildToolInspector.SelectedTool = BuildTool.Paint;
                    break;
                case KeyCode.Alpha4:
                    buildToolInspector.SelectedTool = BuildTool.Picker;
                    break;
                case KeyCode.Alpha5:
                    buildToolInspector.SelectedTool = BuildTool.Cover;
                    break;
            }
        }
 
        private void CycleTool(int direction)
        {
            int toolCount = Enum.GetValues(typeof(BuildTool)).Length;
            int toolIndex = (int)buildToolInspector.SelectedTool + direction;
            toolIndex = (toolIndex + toolCount) % toolCount;
            buildToolInspector.SelectedTool = (BuildTool)toolIndex;
        }
    }
}