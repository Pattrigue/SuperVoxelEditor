using UnityEngine;

namespace SemagGames.SuperVoxelEditor
{
    [CreateAssetMenu(menuName = "Super Voxel Editor/VoxelAsset")]
    public sealed class VoxelAsset : ScriptableObject
    {
        public const int AirId = 0;
        
        [field: SerializeField] public uint ID { get; private set; }
        [field: SerializeField] public Color32 BaseColor { get; private set; } = Color.gray;
    }
}