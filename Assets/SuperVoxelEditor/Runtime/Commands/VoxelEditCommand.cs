namespace SemagGames.SuperVoxelEditor.Commands
{
    public interface IVoxelEditCommand
    {
        void Execute();
        void Undo();
    }
}