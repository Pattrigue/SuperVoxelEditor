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
            this.worldPosition = worldPosition;
            this.voxel = voxel;
            this.chunk = chunk;
        }

        public void Execute()
        {
            // Save previous voxel data for undo
            uint voxelData = chunk.GetVoxelDataFromWorldPosition(worldPosition);
            previousVoxel = Voxel.FromVoxelData(voxelData);

            // Perform the voxel set action
            chunk.SetVoxel(worldPosition, voxel);
        }

        public void Undo()
        {
            chunk.SetVoxel(worldPosition, previousVoxel);
        }
    }
}