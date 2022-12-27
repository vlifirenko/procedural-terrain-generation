using System.Collections.Generic;
using PTG.Model;
using PTG.Model.Config;
using UnityEngine;

namespace PTG.HeightMapModifiers
{
    public class HeightMapModifier_Noise : BaseMapHeightModifier
    {
        [SerializeField] private List<HeightNoisePass> passes;

        public override void Execute(ProcGenConfig globalConfig, int mapResolution, float[,] heightMap, Vector3 heightMapScale,
            byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfig biome = null)
        {
            foreach (var pass in passes)
            {
                for (var y = 0; y < mapResolution; y++)
                {
                    for (var x = 0; x < mapResolution; x++)
                    {
                        if (biomeIndex >= 0 && biomeMap[x, y] != biomeIndex)
                            continue;

                        var noiseValue = Mathf.PerlinNoise(x * pass.noiseScale, y * pass.noiseScale) * 2f - 1f;
                        var newHeight = heightMap[x, y] + noiseValue * pass.heightDelta / heightMapScale.y;

                        heightMap[x, y] = Mathf.Lerp(heightMap[x, y], newHeight, strength);
                    }
                }
            }
        }
    }
}