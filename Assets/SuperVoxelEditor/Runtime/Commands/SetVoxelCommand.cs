using UnityEngine;

namespace SemagGames.SuperVoxelEditor.Commands
{
    [System.Serializable]
    public sealed class SetVoxelCommand : IVoxelEditCommand
    {
        private readonly Chunk chunk;
        
        private readonly Vector3 worldPosition;

        private Voxel voxel;
        private Voxel previousVoxel;

        public SetVoxelCommand(Chunk chunk, Vector3 worldPosition, Voxel voxel)
        {
            this.chunk = chunk;
            this.worldPosition = worldPosition;
            this.voxel = voxel;
        }

        public void Execute()
        {
            // Save previous voxel data for undo
            Vector3Int localPosition = chunk.ToLocalVoxelPosition(worldPosition);
            previousVoxel = Voxel.FromVoxelData(chunk.GetVoxelData(localPosition));

            // Perform the voxel set action
            chunk.SetVoxel(worldPosition, voxel);
        }

        public void Undo()
        {
            chunk.SetVoxel(worldPosition, previousVoxel);
        }
    }
}