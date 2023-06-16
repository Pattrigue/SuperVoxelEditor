using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SemagGames.VoxelEditor
{
    public sealed class VoxelProperties : Singleton<VoxelProperties>
    {
        [SerializeField] private VoxelProperty[] voxelAssets;

        private static VoxelProperty[] VoxelAssets => Instance.voxelAssets;

        public void GetVoxelAssetsInProject()
        {
            voxelAssets = AssetDatabase.FindAssets("t:VoxelAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<VoxelProperty>)
                .OrderBy(asset => asset.ID)
                .ToArray();
        }

        public static VoxelProperty GetPropertyById(uint id)
        {
            return VoxelAssets[id - 1];
        }
    }
}