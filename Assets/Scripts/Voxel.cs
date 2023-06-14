using System;
using UnityEngine;

namespace SemagGames.VoxelEditor
{
    [Serializable]
    public struct Voxel
    {
        public const uint AirId = 0;

        public static readonly Voxel Air = new(AirId, default);

        public uint ID;

        public Color32 Color;

        public Voxel(uint id, Color32 color)
        {
            ID = id;
            Color = color;
        }

        public VoxelAsset Asset => VoxelAssetDatabase.GetVoxelAsset(ID);

        public static bool operator ==(Voxel a, Voxel b)
        {
            return a.ID == b.ID;
        }

        public static bool operator !=(Voxel a, Voxel b)
        {
            return !(a == b);
        }

        public bool Equals(Voxel other)
        {
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            return obj is Voxel other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}