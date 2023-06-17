using UnityEngine;

namespace SemagGames.VoxelEditor
{
    [CreateAssetMenu(menuName = "VoxelProperty")]
    public sealed class VoxelProperty : ScriptableObject
    {
        [field: SerializeField] public uint ID { get; private set; }
    }
}