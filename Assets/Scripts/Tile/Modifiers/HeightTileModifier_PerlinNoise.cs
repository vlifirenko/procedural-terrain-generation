using System.Collections.Generic;
using PTG.Model;
using PTG.Model.Config;
using UnityEngine;

namespace PTG.Tile.Modifiers
{
    public class HeightTileModifier_PerlinNoise : BaseTileModifier
    {
        [SerializeField] private List<PerlinOctave> heightNoise = new List<PerlinOctave>();
        [SerializeField] private Vector2 heightNoiseScale = new Vector2(8f, 8f);

        public override void Execute(int numVertsPerSide, Vector3[] vertices, Color[] vertexColors, Vector3[] normals,
            Texture2D texture,
            ProcGenTileConfig config, ProcGenTile tile)
        {
            if (heightNoise.Count == 0)
                heightNoise.Add(new PerlinOctave() {amplitude = 1f, frequency = 1f});

            var originPoint = tile.transform.position;

            for (var octave = 0; octave < heightNoise.Count; octave++)
            {
                var octaveConfig = heightNoise[octave];
                var currentScale = heightNoiseScale * octaveConfig.frequency;

                for (var row = 0; row < numVertsPerSide; row++)
                {
                    var rowProgress = originPoint.z / config.tileSize + (float) row / (numVertsPerSide - 1);
                    rowProgress *= currentScale.x;

                    for (var col = 0; col < numVertsPerSide; col++)
                    {
                        var colProgress = originPoint.x / config.tileSize + (float) col / (numVertsPerSide - 1);
                        colProgress *= currentScale.y;

                        var vertIndex = row * numVertsPerSide + col;
                        var height = config.maxHeight * Mathf.PerlinNoise(rowProgress, colProgress);

                        vertices[vertIndex].y += height * octaveConfig.amplitude;
                    }
                }
            }
        }
    }
}