using System;
using UnityEngine;

namespace SemagGames.VoxelEditor
{
    [Serializable]
    public struct Voxel
    {
        public const uint AirId = 0;

        public static readonly Voxel Air = new(AirId, default);

        public uint id;

        public Color32 color;

        public Voxel(uint id, Color32 color)
        {
            this.id = id;
            this.color = color;
        }

        public VoxelProperty Property => VoxelProperties.GetPropertyById(id);

        public static bool operator ==(Voxel a, Voxel b)
        {
            return a.id == b.id && a.color.r == b.color.r && a.color.g == b.color.g && a.color.b == b.color.b && a.color.a == b.color.a;
        }

        public static bool operator !=(Voxel a, Voxel b)
        {
            return !(a == b);
        }

        public bool Equals(Voxel other)
        {
            return id == other.id;
        }

        public override bool Equals(object obj)
        {
            return obj is Voxel other && Equals(other);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }
}