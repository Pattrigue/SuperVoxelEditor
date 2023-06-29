using UnityEngine;

namespace SemagGames.SuperVoxelEditor
{
    public static class Color32Extensions
    {
        public static bool IsSameColor(this Color32 colorA, Color32 colorB)
        {
            return colorA.r == colorB.r && colorA.g == colorB.g && colorA.b == colorB.b;
        }
    }
}