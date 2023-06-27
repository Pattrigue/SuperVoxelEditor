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
    
            VoxelProperty voxelProperty = GetVoxelProperty(voxel.propertyId);
    
            if (voxelProperty != null)
            {
                volume.VoxelProperty = voxelProperty;
            }
                
            volume.ColorPicker.SetColor(voxel.colorId);
            
            return true;
        }
    
        private static VoxelProperty GetVoxelProperty(uint propertyId)
        {
            string[] voxelProperties = AssetDatabase.FindAssets("t:VoxelProperty");
                
            foreach (string voxelProperty in voxelProperties)
            {
                var voxelPropertyObject = AssetDatabase.LoadAssetAtPath<VoxelProperty>(AssetDatabase.GUIDToAssetPath(voxelProperty));
                    
                if (voxelPropertyObject.ID == propertyId)
                {
                    return voxelPropertyObject;
                }
            }
    
            return null;
        }
    }
}