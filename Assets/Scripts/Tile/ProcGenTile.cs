using System.Collections.Generic;
using PTG.Model;
using PTG.Model.Config;
using PTG.Model.Enum;
using UnityEngine;
using UnityEngine.Rendering;

namespace PTG.Tile
{
    public class ProcGenTile : MonoBehaviour
    {
        [SerializeField] private ProcGenTileConfig config;
        [SerializeField] private GameObject modifiers;

        private MeshFilter _linkedMeshFilter;

        private float TileSize => config.tileSize;
        private ETerrainResolution TerrainResolution => config.terrainResolution;
        private float MaxHeight => config.maxHeight;
        private EPaintingMode PaintingMode => config.paintingMode;
        private ETextureResolution TextureResolution => config.textureResolution;

        private void Awake() => _linkedMeshFilter = GetComponent<MeshFilter>();

        private void Start() => PerformMeshGeneration();

        private void PerformMeshGeneration()
        {
            var numVertsPerSide = (int) TerrainResolution;
            var totalNumVerts = numVertsPerSide * numVertsPerSide;
            var vertexSpacing = TileSize / (numVertsPerSide - 1);
            var halfTileSize = TileSize * 0.5f;

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

            for (var vertIndex = 0; vertIndex < totalNumVerts; vertIndex++)
            {
                meshUVs[vertIndex] = new Vector2(
                    (vertices[vertIndex].x + halfTileSize) / TileSize,
                    (vertices[vertIndex].z + halfTileSize) / TileSize);
            }

            var allModifiers = modifiers.GetComponents<BaseTileModifier>();
            foreach (var modifier in allModifiers)
            {
                if (modifier.Phase == EProcGenPhase.Initial)
                    modifier.Execute(numVertsPerSide, vertices, vertexColors, null, null, config, this);
            }

            //PerformMeshGeneration_Height(numVertsPerSide, vertices);

            var mesh = new UnityEngine.Mesh
                {indexFormat = triangleIndices.Length > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16};

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, meshUVs);
            mesh.SetTriangles(triangleIndices, 0);

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            //PerformMeshGeneration_Paint(numVertsPerSide, vertices, vertexColors, mesh.normals);

            var vertexNormals = mesh.normals;
            foreach (var modifier in allModifiers)
            {
                if (modifier.Phase == EProcGenPhase.AfterNormals)
                    modifier.Execute(numVertsPerSide, vertices, vertexColors, vertexNormals, null, config, this);
            }

            mesh.SetColors(vertexColors);

            Texture2D finalTexture = null;
            if (PaintingMode == EPaintingMode.TextureBased)
            {
                var initialTexture = new Texture2D(numVertsPerSide, numVertsPerSide, TextureFormat.RGB24, true);
                initialTexture.SetPixels(vertexColors);
                initialTexture.Apply();

                finalTexture = initialTexture;

                if ((int) TextureResolution != (int) TerrainResolution)
                {
                    var targetRT = new RenderTexture((int) TextureResolution, (int) TextureResolution, 24);
                    var previousRT = RenderTexture.active;

                    RenderTexture.active = targetRT;
                    Graphics.Blit(initialTexture, targetRT);

                    finalTexture = new Texture2D((int) TextureResolution, (int) TextureResolution, TextureFormat.RGB24, true);
                    finalTexture.ReadPixels(new Rect(0, 0, (int) TextureResolution, (int) TextureResolution), 0, 0);
                    finalTexture.Apply();

                    finalTexture.filterMode = FilterMode.Bilinear;
                    finalTexture.wrapMode = TextureWrapMode.Clamp;

                    RenderTexture.active = previousRT;

                    _linkedMeshFilter.GetComponent<MeshRenderer>().material.SetTexture("_BaseMap", finalTexture);
                }
            }

            foreach (var modifier in allModifiers)
            {
                if (modifier.Phase == EProcGenPhase.PostProcess)
                    modifier.Execute(numVertsPerSide, vertices, vertexColors, vertexNormals, finalTexture, config, this);
            }

            _linkedMeshFilter.mesh = mesh;
        }
    }
}