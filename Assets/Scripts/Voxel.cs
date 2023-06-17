using System;
using UnityEngine;

namespace SemagGames.VoxelEditor
{
    [Serializable]
    public struct Voxel
    {
        public const uint AirId = 0;

        public static readonly Voxel Air = new(AirId, default);
        public static readonly uint AirVoxelData = Air.ToVoxelData();

        public uint id;

        public Color32 color;

        public Voxel(uint id, Color32 color)
        {
            this.id = id;
            this.color = color;
        }

        public VoxelProperty Property => VoxelProperties.GetPropertyById(id);

        public static uint ExtractId(uint voxelData)
        {
            return voxelData >> 20;
        }
    
        public static Color32 ExtractColor(uint voxelData)
        {
            return new Color32(
                (byte) (((voxelData >> 13) & 0x7F) * 2),
                (byte) (((voxelData >> 6) & 0x7F) * 2),
                (byte) ((voxelData & 0x3F) * 4), 
                255
            );
        }
    
        public uint ToVoxelData()
        {
            uint voxelData = (id << 20) |
                             (((uint)color.r / 2) << 13) |
                             (((uint)color.g / 2) << 6) |
                             ((uint)color.b / 4);
            return voxelData;
        }
        
        public static Voxel FromVoxelData(uint voxelData)
        {
            uint id = ExtractId(voxelData);
            Color32 color = ExtractColor(voxelData);

            return new Voxel(id, color);
        }
        
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