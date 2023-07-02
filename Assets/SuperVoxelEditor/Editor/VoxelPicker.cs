using SemagGames.SuperVoxelEditor;
using UnityEditor;
using UnityEngine;

namespace SuperVoxelEditor.Editor
{
    public static class VoxelPicker
    {
        public static bool PickVoxelAtPosition(VoxelVolume volume, Vector3 voxelPosition)
        {
            if (!volume.TryGetVoxel(voxelPosition, out Voxel voxel)) return false;
    
            VoxelAsset voxelAsset = GetVoxelAsset(voxel.id);
    
            if (voxelAsset != null)
            {
                volume.VoxelAsset = voxelAsset;
            }
                
            return true;
        }
    
        private static VoxelAsset GetVoxelAsset(uint assetId)
        {
            string[] voxelAssets = AssetDatabase.FindAssets("t:VoxelAsset");
                
            foreach (string voxelAsset in voxelAssets)
            {
                var voxelAssetObject = AssetDatabase.LoadAssetAtPath<VoxelAsset>(AssetDatabase.GUIDToAssetPath(voxelAsset));
                    
                if (voxelAssetObject.ID == assetId)
                {
                    return voxelAssetObject;
                }
            }
    
            return null;
        }
    }
}