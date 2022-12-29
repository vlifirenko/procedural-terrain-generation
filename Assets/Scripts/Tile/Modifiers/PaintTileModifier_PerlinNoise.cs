using System.Collections.Generic;
using PTG.Model;
using PTG.Model.Config;
using UnityEngine;
using UnityEngine.Serialization;

namespace PTG.Tile.Modifiers
{
    public class PaintTileModifier_PerlinNoise : BaseTileModifier
    {
        [SerializeField] private Color baseColor;
        [SerializeField] private List<ColoredPerlinOctave> configs;

        public override void Execute(int numVertsPerSide, Vector3[] vertices, Color[] vertexColors, Vector3[] normals,
            Texture2D texture,
            ProcGenTileConfig config, ProcGenTile tile)
        {
            var originPoint = tile.transform.position;

            for (var row = 0; row < numVertsPerSide; row++)
            {
                var rowProgress = originPoint.z / config.tileSize + (float) row / (numVertsPerSide - 1);

                for (var col = 0; col < numVertsPerSide; col++)
                {
                    var colProgress = originPoint.x / config.tileSize + (float) col / (numVertsPerSide - 1);
                    var vertIndex = row * numVertsPerSide + col;

                    var vertexColor = baseColor;
                    foreach (var colorConfig in configs)
                    {
                        var noise = Mathf.PerlinNoise(
                            colorConfig.scale * rowProgress,
                            colorConfig.scale * colProgress);

                        if (noise >= colorConfig.threshold)
                            vertexColor = Color.Lerp(vertexColor, colorConfig.noiseColor, colorConfig.intensity);
                    }

                    vertexColors[vertIndex] = vertexColor;
                }
            }
        }
    }
}