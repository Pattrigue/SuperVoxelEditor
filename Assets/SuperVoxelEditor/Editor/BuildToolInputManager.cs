﻿using System;
using SemagGames.SuperVoxelEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
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
                    buildToolInspector.SelectedTool = BuildToolType.Attach;
                    break;
                case KeyCode.Alpha2:
                    buildToolInspector.SelectedTool = BuildToolType.Erase;
                    break;
                case KeyCode.Alpha3:
                    buildToolInspector.SelectedTool = BuildToolType.Paint;
                    break;
                case KeyCode.Alpha4:
                    buildToolInspector.SelectedTool = BuildToolType.Picker;
                    break;
            }
        }
 
        private void CycleTool(int direction)
        {
            int toolCount = Enum.GetValues(typeof(BuildToolType)).Length;
            int toolIndex = (int)buildToolInspector.SelectedTool + direction;
            toolIndex = (toolIndex + toolCount) % toolCount;
            buildToolInspector.SelectedTool = (BuildToolType)toolIndex;
        }
    }
}