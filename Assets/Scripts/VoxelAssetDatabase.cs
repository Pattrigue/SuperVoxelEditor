using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace SemagGames.VoxelEditor
{
    public sealed class VoxelAssetDatabase : Singleton<VoxelAssetDatabase>
    {
        [SerializeField] private VoxelAsset[] voxelAssets;

        private static VoxelAsset[] VoxelAssets => Instance.voxelAssets;

        [Button]
        private void GetVoxelAssetsInProject()
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