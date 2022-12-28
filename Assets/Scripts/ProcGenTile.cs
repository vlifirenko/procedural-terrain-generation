using PTG.Model.Enum;
using UnityEngine;
using UnityEngine.Rendering;

namespace PTG
{
    public class ProcGenTile : MonoBehaviour
    {
        [SerializeField] private float tileSize = 200f;
        [SerializeField] private EResolution resolution = EResolution.Size_64x64;

        private MeshFilter _linkedMeshFilter;

        private void Awake() => _linkedMeshFilter = GetComponent<MeshFilter>();

        private void Start() => PerformMeshGeneration();

        private void PerformMeshGeneration()
        {
            Vector3[] vertices;
            Vector2[] meshUVs;
            Color[] vertexColors;
            int[] triangleIndices;

            var numVertsPerSide = (int) resolution;
            var totalNumVerts = numVertsPerSide * numVertsPerSide;

            vertices = new Vector3[totalNumVerts];
            meshUVs = new Vector2[totalNumVerts];
            vertexColors = new Color[totalNumVerts];
            triangleIndices = new int[(numVertsPerSide - 1) * (numVertsPerSide - 1) * 2 * 3];

            var mesh = new Mesh();

            mesh.indexFormat = triangleIndices.Length > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, meshUVs);
            mesh.SetTriangles(triangleIndices, 0);

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            mesh.SetColors(vertexColors);

            _linkedMeshFilter.mesh = mesh;
        }
    }
}