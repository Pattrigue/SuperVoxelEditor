using System;
using UnityEngine;

namespace SemagGames.SuperVoxelEditor
{
    [Serializable]
    public struct Voxel
    {
        // Byte layout of voxel data:
        // 0-7: Property ID
        // 8-15: R
        // 16-23: G
        // 24-31: B
        // 32: Unused
        
        public static readonly Voxel Air = new(VoxelProperty.AirId, default, default, default);
        public static readonly uint AirVoxelData = Air.ToVoxelData();

        public uint propertyId;
        public byte r;
        public byte g;
        public byte b;

        public Voxel(uint propertyId, byte r, byte g, byte b)
        {
            this.propertyId = propertyId;
            this.r = r;
            this.g = g;
            this.b = b;
        }
    
        public Voxel(uint propertyId, Color color)
        {
            this.propertyId = propertyId;
            r = (byte)(color.r * 255);
            g = (byte)(color.g * 255);
            b = (byte)(color.b * 255);
        }
    
        public Voxel(uint propertyId, Color32 color)
        {
            this.propertyId = propertyId;
            r = color.r;
            g = color.g;
            b = color.b;
        }

        public Color32 GetColor() => new(r, g, b, 255);

        public static uint GetPropertyId(uint voxelData) => voxelData >> 22;

        public static uint GetRedChannel(uint voxelData) => ((voxelData >> 15) & 0x7F) * 2;
        public static uint GetGreenChannel(uint voxelData) => ((voxelData >> 8) & 0x7F) * 2;
        public static uint GetBlueChannel(uint voxelData) => ((voxelData >> 1) & 0x7F) * 2;

        
        public static Color GetColor(uint voxelData)
        {
            byte redChannel = (byte)GetRedChannel(voxelData);
            byte greenChannel = (byte)GetGreenChannel(voxelData);
            byte blueChannel = (byte)GetBlueChannel(voxelData);

            return new Color(redChannel / 255f, greenChannel / 255f, blueChannel / 255f);
        }
    
        public static Color32 GetColor32(uint voxelData)
        {
            byte redChannel = (byte)GetRedChannel(voxelData);
            byte greenChannel = (byte)GetGreenChannel(voxelData);
            byte blueChannel = (byte)GetBlueChannel(voxelData);

            return new Color32(redChannel, greenChannel, blueChannel, 255);
        }
        
        public uint ToVoxelData()
        {
            uint voxelData = (propertyId << 22) |
                             ((uint)Math.Round(r / 255.0 * 127) << 15) |
                             ((uint)Math.Round(g / 255.0 * 127) << 8) |
                             ((uint)Math.Round(b / 255.0 * 127) << 1);

            return voxelData;
        }
    
        public static Voxel FromVoxelData(uint voxelData)
        {
            uint id = GetPropertyId(voxelData);
            byte redChannel = (byte)(GetRedChannel(voxelData) * 2);
            byte greenChannel = (byte)(GetGreenChannel(voxelData) * 2);
            byte blueChannel = (byte)(GetBlueChannel(voxelData) * 2);

            return new Voxel(id, redChannel, greenChannel, blueChannel);
        }
        
        public static bool IsSameColor(uint voxelDataA, uint voxelDataB)
        {
            uint redChannelA = GetRedChannel(voxelDataA);
            uint greenChannelA = GetGreenChannel(voxelDataA);
            uint blueChannelA = GetBlueChannel(voxelDataA);
            
            uint redChannelB = GetRedChannel(voxelDataB);
            uint greenChannelB = GetGreenChannel(voxelDataB);
            uint blueChannelB = GetBlueChannel(voxelDataB);

            return redChannelA == redChannelB && greenChannelA == greenChannelB && blueChannelA == blueChannelB;
        }

        public static bool IsAir(uint voxelData) => GetPropertyId(voxelData) == VoxelProperty.AirId;

        public static bool IsAirId(uint voxelPropertyId) => voxelPropertyId == VoxelProperty.AirId;

        public static bool operator ==(Voxel a, Voxel b)
        {
            return a.propertyId == b.propertyId && a.r == b.r && a.g == b.g && a.b == b.b;
        }

        public static bool operator !=(Voxel a, Voxel b)
        {
            return !(a == b);
        }

        public bool Equals(Voxel other)
        {
            return propertyId == other.propertyId && r == other.r && g == other.g && b == other.b;
        }

        public override bool Equals(object obj)
        {
            return obj is Voxel other && Equals(other);
        }

        public override int GetHashCode()
        {
            return propertyId.GetHashCode();
        }
    }
}