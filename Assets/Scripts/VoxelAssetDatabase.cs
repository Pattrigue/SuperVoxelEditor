using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SemagGames.VoxelEditor
{
    public sealed class VoxelAssetDatabase : Singleton<VoxelAssetDatabase>
    {
        [SerializeField] private VoxelAsset[] voxelAssets;

        private static VoxelAsset[] VoxelAssets => Instance.voxelAssets;

        public void GetVoxelAssetsInProject()
        {
            voxelAssets = AssetDatabase.FindAssets("t:VoxelAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<VoxelAsset>)
                .OrderBy(asset => asset.ID)
                .ToArray();
        }

        public static VoxelAsset GetVoxelAsset(uint id)
        {
            return VoxelAssets[id - 1];
        }
    }
}