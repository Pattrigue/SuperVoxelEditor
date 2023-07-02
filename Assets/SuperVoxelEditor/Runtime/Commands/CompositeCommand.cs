using System.Collections.Generic;

namespace SemagGames.SuperVoxelEditor.Commands
{
    public sealed class CompositeCommand : ICompositeCommand
    {
        private readonly List<IVoxelEditCommand> commands = new();

        public void AddCommand(IVoxelEditCommand command)
        {
            commands.Add(command);
        }

        public void Execute()
        {
            foreach (var command in commands)
            {
                command.Execute();
            }
        }

        public void Undo()
        {
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                commands[i].Undo();
            }
        }
    }
}