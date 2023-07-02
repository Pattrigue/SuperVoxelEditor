using System;
using UnityEngine;

namespace SemagGames.SuperVoxelEditor
{
    [Serializable]
    public struct Voxel
    {
        // Byte layout of voxel data:
        // 0-9:   VoxelAsset ID (0-1023)
        // 10-16: R (0-127)
        // 17-23: G (0-127)
        // 24-30: B (0-127)
        // 31: Unused
        
        public static readonly Voxel Air = new(VoxelAsset.AirId, default, default, default);
        public static readonly uint AirVoxelData = Air.ToVoxelData();

        public uint id;
        public byte r;
        public byte g;
        public byte b;

        public Voxel(uint id, byte r, byte g, byte b)
        {
            this.id = id;
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public Voxel(VoxelAsset voxelAsset)
        {
            id = voxelAsset.ID;
            r = voxelAsset.Color.r;
            g = voxelAsset.Color.g;
            b = voxelAsset.Color.b;
        }
    
        public Voxel(uint id, Color color)
        {
            this.id = id;
            r = (byte)(color.r * 255);
            g = (byte)(color.g * 255);
            b = (byte)(color.b * 255);
        }
    
        public Voxel(uint id, Color32 color)
        {
            this.id = id;
            r = color.r;
            g = color.g;
            b = color.b;
        }

        public Color32 GetColor()
        {
            return new Color32(r, g, b, 255);
        }

        public void SetColor(Color32 color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
        }

        public static uint GetId(uint voxelData) => voxelData >> 22;

        public static uint GetRedChannel(uint voxelData) => ((voxelData >> 15) & 0x7F) * 255 / 127;
        
        public static uint GetGreenChannel(uint voxelData) => ((voxelData >> 8) & 0x7F) * 255 / 127;
        
        public static uint GetBlueChannel(uint voxelData) => ((voxelData >> 1) & 0x7F) * 255 / 127;

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
            uint voxelData = (id << 22) |
                             ((uint)Math.Round(r / 255.0 * 127) << 15) |
                             ((uint)Math.Round(g / 255.0 * 127) << 8) |
                             ((uint)Math.Round(b / 255.0 * 127) << 1);

            return voxelData;
        }

        public static Voxel FromVoxelData(uint voxelData)
        {
            uint id = GetId(voxelData);
            byte redChannel = (byte)GetRedChannel(voxelData);
            byte greenChannel = (byte)GetGreenChannel(voxelData);
            byte blueChannel = (byte)GetBlueChannel(voxelData);

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

        public static bool IsAir(uint voxelData) => GetId(voxelData) == VoxelAsset.AirId;

        public static bool IsAirId(uint voxelId) => voxelId == VoxelAsset.AirId;

        public static bool operator ==(Voxel a, Voxel b)
        {
            return a.id == b.id;
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