﻿using System;

namespace SemagGames.SuperVoxelEditor.Commands
{
    [Serializable]
    public sealed class CommandManager
    {
        private const int BufferSize = 64;
        
        private readonly IVoxelEditCommand[] commandBuffer = new IVoxelEditCommand[BufferSize];
        private readonly IVoxelEditCommand[] redoBuffer = new IVoxelEditCommand[BufferSize];
        
        private int undoIndex = -1;
        private int redoIndex = -1;

        public void Do(IVoxelEditCommand command)
        {
            command.Execute();
        
            undoIndex = (undoIndex + 1) % BufferSize;
            commandBuffer[undoIndex] = command;
        
            redoIndex = -1; // Invalidate the redo stack when we do a new action
            Array.Clear(redoBuffer, 0, redoBuffer.Length); // Optional: Free up some memory
        }

        public void Undo()
        {
            if (undoIndex == -1 || commandBuffer[undoIndex] == null) return; // Nothing to undo

            IVoxelEditCommand command = commandBuffer[undoIndex];
            command.Undo();

            redoIndex = (redoIndex + 1) % BufferSize;
            redoBuffer[redoIndex] = command;
        
            commandBuffer[undoIndex] = null;
            undoIndex = (undoIndex - 1 + BufferSize) % BufferSize;
        }

        public void Redo()
        {
            if (redoIndex == -1 || redoBuffer[redoIndex] == null) return; // Nothing to redo

            IVoxelEditCommand command = redoBuffer[redoIndex];
            command.Execute();

            undoIndex = (undoIndex + 1) % BufferSize;
            commandBuffer[undoIndex] = command;
        
            redoBuffer[redoIndex] = null;
            redoIndex = (redoIndex - 1 + BufferSize) % BufferSize;
        }
    }
}