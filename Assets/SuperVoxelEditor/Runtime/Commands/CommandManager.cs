using System.Collections.Generic;

namespace SemagGames.SuperVoxelEditor.Commands
{
    [System.Serializable]
    public sealed class CommandManager
    {
        private readonly Stack<IVoxelEditCommand> commandStack = new();
        private readonly Stack<IVoxelEditCommand> redoStack = new();

        public void Do(IVoxelEditCommand command)
        {
            command.Execute();
            commandStack.Push(command);
            redoStack.Clear();
        }

        public void Undo()
        {
            if (commandStack.Count <= 0) return;
            
            IVoxelEditCommand command = commandStack.Pop();
            command.Undo();
            redoStack.Push(command);
        }

        public void Redo()
        {
            if (redoStack.Count <= 0) return;
            
            IVoxelEditCommand command = redoStack.Pop();
            command.Execute();
            commandStack.Push(command);
        }
    }
}