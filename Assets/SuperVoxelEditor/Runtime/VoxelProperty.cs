using UnityEngine;

namespace SemagGames.SuperVoxelEditor
{
    [CreateAssetMenu(menuName = "VoxelProperty")]
    public sealed class VoxelProperty : ScriptableObject
    {
        public const int AirId = 0;
        
        [field: SerializeField] public uint ID { get; private set; }
    }
}