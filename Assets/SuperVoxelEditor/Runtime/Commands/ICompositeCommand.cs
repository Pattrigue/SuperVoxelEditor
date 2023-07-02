namespace SemagGames.SuperVoxelEditor.Commands
{
    public interface ICompositeCommand : IVoxelEditCommand
    {
        void AddCommand(IVoxelEditCommand command);
    }
}