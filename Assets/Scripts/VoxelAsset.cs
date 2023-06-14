using UnityEngine;

namespace SemagGames.VoxelEditor
{
    [CreateAssetMenu(menuName = "Voxel")]
    public sealed class VoxelAsset : ScriptableObject
    {
        [field: SerializeField] public uint ID { get; private set; }
    }
}