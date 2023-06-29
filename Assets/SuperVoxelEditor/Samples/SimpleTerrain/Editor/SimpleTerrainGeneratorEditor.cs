﻿using UnityEditor;
using UnityEngine;

namespace SemagGames.SuperVoxelEditor.Samples.SimpleTerrain.Editor
{
    [CustomEditor(typeof(SimpleTerrainGenerator))]
    public sealed class SimpleTerrainGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Rebuild"))
            {
                ((SimpleTerrainGenerator)target).Rebuild();
            }
        }
    }
}