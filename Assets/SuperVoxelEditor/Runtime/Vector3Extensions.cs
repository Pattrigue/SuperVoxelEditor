using UnityEngine;

namespace SemagGames.SuperVoxelEditor
{
    public static class Vector3Extensions
    {
        public static Vector3 SnapToVoxelGrid(this Vector3 position)
        {
            return new Vector3(
                Mathf.FloorToInt(position.x) + 0.5f,
                Mathf.FloorToInt(position.y) + 0.5f,
                Mathf.FloorToInt(position.z) + 0.5f
            );
        }
    }
}