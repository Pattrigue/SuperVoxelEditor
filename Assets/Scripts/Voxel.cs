using System;

namespace SemagGames.VoxelEditor
{
    [Serializable]
    public struct Voxel
    {
        public static readonly Voxel Air = new(VoxelProperty.AirId, default);
        public static readonly uint AirVoxelData = Air.ToVoxelData();

        public uint propertyId;
        public uint colorId;

        public Voxel(uint propertyId, uint colorId)
        {
            this.propertyId = propertyId;
            this.colorId = colorId;
        }

        public static uint GetPropertyId(uint voxelData) => voxelData >> 20;

        public static uint GetColorId(uint voxelData) => (voxelData >> 12) & 0xFF;

        public uint ToVoxelData()
        {
            uint voxelData = (propertyId << 20) |
                             (colorId << 12);
            
            return voxelData;
        }
        
        public static Voxel FromVoxelData(uint voxelData)
        {
            uint id = GetPropertyId(voxelData);
            uint colorId = GetColorId(voxelData);

            return new Voxel(id, colorId);
        }
        
        public static bool IsAir(uint voxelData) => voxelData == AirVoxelData;

        public static bool IsAirId(uint voxelPropertyId) => voxelPropertyId == VoxelProperty.AirId;

        public static bool operator ==(Voxel a, Voxel b)
        {
            return a.propertyId == b.propertyId && a.colorId == b.colorId;
        }

        public static bool operator !=(Voxel a, Voxel b)
        {
            return !(a == b);
        }

        public bool Equals(Voxel other)
        {
            return propertyId == other.propertyId;
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