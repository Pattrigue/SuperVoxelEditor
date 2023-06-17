using UnityEngine;

namespace SemagGames.VoxelEditor
{
    [CreateAssetMenu(menuName = "VoxelProperty")]
    public sealed class VoxelProperty : ScriptableObject
    {
        public const int AirId = 0;
        
        [field: SerializeField] public uint ID { get; private set; }
    }
}