using System.Collections.Generic;
using PTG.Model;
using PTG.Model.Enum;
using UnityEngine;
using UnityEngine.Rendering;

namespace PTG
{
    public class ProcGenTile : MonoBehaviour
    {
        [SerializeField] private float tileSize = 200f;
        [SerializeField] private EResolution resolution = EResolution.Size_64x64;
        [SerializeField] private float maxHeight = 100f;
        [SerializeField] private List<PerlinOctave> heightNoise = new List<PerlinOctave>();
        [SerializeField] private Vector2 heightNoiseScale = new Vector2(8f, 8f);

        private MeshFilter _linkedMeshFilter;

        private void Awake() => _linkedMeshFilter = GetComponent<MeshFilter>();

        private void Start() => PerformMeshGeneration();

        private void PerformMeshGeneration()
        {
            var numVertsPerSide = (int) resolution;
            var totalNumVerts = numVertsPerSide * numVertsPerSide;
            var vertexSpacing = tileSize / (numVertsPerSide - 1);
            var halfTileSize = tileSize * 0.5f;

            var vertices = new Vector3[totalNumVerts];
            var meshUVs = new Vector2[totalNumVerts];
            var vertexColors = new Color[totalNumVerts];
            var triangleIndices = new int[(numVertsPerSide - 1) * (numVertsPerSide - 1) * 2 * 3];

            for (var row = 0; row < numVertsPerSide; row++)
            {
                for (var col = 0; col < numVertsPerSide; col++)
                {
                    var vertIndex = (row * numVertsPerSide) + col;

                    vertices[vertIndex] = new Vector3(
                        col * vertexSpacing - halfTileSize,
                        0f,
                        row * vertexSpacing - halfTileSize);

                    vertexColors[vertIndex] = Color.green;

                    if (row < numVertsPerSide - 1 && col < numVertsPerSide - 1)
                    {
                        var baseIndex = row * (numVertsPerSide - 1) + col;

                        triangleIndices[baseIndex * 6 + 0] = vertIndex;
                        triangleIndices[baseIndex * 6 + 1] = vertIndex + numVertsPerSide;
                        triangleIndices[baseIndex * 6 + 2] = vertIndex + numVertsPerSide + 1;

                        triangleIndices[baseIndex * 6 + 3] = vertIndex;
                        triangleIndices[baseIndex * 6 + 4] = vertIndex + numVertsPerSide + 1;
                        triangleIndices[baseIndex * 6 + 5] = vertIndex + 1;
                    }
                }
            }

            PerformMeshGeneration_Height(numVertsPerSide, vertices);

            for (var vertIndex = 0; vertIndex < totalNumVerts; vertIndex++)
            {
                meshUVs[vertIndex] = new Vector2(
                    (vertices[vertIndex].x + halfTileSize) / tileSize,
                    (vertices[vertIndex].z + halfTileSize) / tileSize);
            }

            var mesh = new Mesh {indexFormat = triangleIndices.Length > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16};

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, meshUVs);
            mesh.SetTriangles(triangleIndices, 0);

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            PerformMeshGeneration_Paint(numVertsPerSide, vertices, vertexColors, mesh.normals);

            mesh.SetColors(vertexColors);

            _linkedMeshFilter.mesh = mesh;
        }

        private void PerformMeshGeneration_Height(in int numVertsPerSide, Vector3[] vertices)
        {
            if (heightNoise.Count == 0)
                heightNoise.Add(new PerlinOctave() {amplitude = 1f, frequency = 1f});

            for (var octave = 0; octave < heightNoise.Count; octave++)
            {
                var octaveConfig = heightNoise[octave];
                var currentScale = heightNoiseScale * octaveConfig.frequency;

                for (var row = 0; row < numVertsPerSide; row++)
                {
                    var rowProgress = (transform.position.z / tileSize) + (float) row / (numVertsPerSide - 1);
                    rowProgress *= currentScale.x;

                    for (var col = 0; col < numVertsPerSide; col++)
                    {
                        var colProgress = transform.position.x / tileSize + (float) col / (numVertsPerSide - 1);
                        colProgress *= currentScale.y;

                        var vertIndex = row * numVertsPerSide + col;
                        var height = maxHeight * Mathf.PerlinNoise(rowProgress, colProgress);

                        vertices[vertIndex].y += height * octaveConfig.amplitude;
                    }
                }
            }
        }

        private void PerformMeshGeneration_Paint(in int numVertsPerSide, Vector3[] vertices, Color[] vertexColors,
            Vector3[] normals)
        {
            for (var row = 0; row < numVertsPerSide; row++)
            {
                for (var col = 0; col < numVertsPerSide; col++)
                {
                    var vertIndex = row * numVertsPerSide + col;
                    var heightProgress = vertices[vertIndex].y / maxHeight;

                    //vertexColors[vertIndex] = Color.Lerp(Color.green, Color.white, heightProgress);

                    //vertexColors[vertIndex] = Color.Lerp(Color.red, Color.white, normals[vertIndex].y);
                }
            }
        }
    }
}