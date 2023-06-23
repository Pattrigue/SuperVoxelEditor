using UnityEngine;

namespace SemagGames.SuperVoxelEditor
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public sealed class ChunkCollider : MonoBehaviour
    {
        private MeshCollider meshCollider;
        private MeshFilter meshFilter;

        private void Awake()
        {
            meshCollider = GetComponent<MeshCollider>();
            meshFilter = GetComponent<MeshFilter>();
        }

        public void Rebuild()
        {
            Debug.Log($"Rebuilding chunk collider {transform.name}", gameObject);
            meshCollider.sharedMesh = meshFilter.sharedMesh;
        }
    }
}