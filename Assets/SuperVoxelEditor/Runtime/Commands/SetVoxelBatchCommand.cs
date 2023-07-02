using System.Collections.Generic;
using UnityEngine;

namespace SemagGames.SuperVoxelEditor.Commands
{
    public sealed class SetVoxelBatchCommand : IVoxelEditCommand
    {
        private readonly VoxelVolume volume;

        private readonly List<SetVoxelCommand> setVoxelCommands;
        private readonly Vector3[] worldPositions;

        private readonly Voxel voxel;
        
        public SetVoxelBatchCommand(VoxelVolume volume, Vector3[] worldPositions, Voxel voxel)
        {
            this.volume = volume;
            this.worldPositions = worldPositions;
            this.voxel = voxel;
            
            setVoxelCommands = new List<SetVoxelCommand>(worldPositions.Length);
        }

        public void Execute()
        {
            foreach (Vector3 worldPosition in worldPositions)
            {
                Chunk chunk = volume.GetOrCreateChunk(worldPosition);
                
                SetVoxelCommand setVoxelCommand = new SetVoxelCommand(chunk, worldPosition, voxel);
                setVoxelCommand.Execute();
                setVoxelCommands.Add(setVoxelCommand);
            }
        }

        public void Undo()
        {
            foreach (SetVoxelCommand setVoxelCommand in setVoxelCommands)
            {
                setVoxelCommand.Undo();
            }
        }
    }
}