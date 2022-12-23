using PTG.Model.Config;
using UnityEngine;

namespace PTG.HeightMapModifiers
{
    public class HeightMapModifier_Noise : BaseMapHeightModifier
    {
        [SerializeField] private float heightDelta;
        [SerializeField] private float xScale = 1f;
        [SerializeField] private float yScale = 1f;
        [SerializeField] private int numPasses = 1;
        [SerializeField] private float xScaleVariationPerPass = 2f;
        [SerializeField] private float yScaleVariationPerPass = 2f;
        [SerializeField] private float heightDeltaVariationPerPass = 0.5f;

        public override void Execute(int mapResolution, float[,] heightMap, Vector3 heightMapScale, byte[,] biomeMap = null,
            int biomeIndex = -1, BiomeConfig biome = null)
        {
            var workingXScale = xScale;
            var workingYScale = yScale;
            var workingHeightDelta = heightDelta;

            for (var pass = 0; pass < numPasses; pass++)
            {
                for (var y = 0; y < mapResolution; y++)
                {
                    for (var x = 0; x < mapResolution; x++)
                    {
                        if (biomeIndex >= 0 && biomeMap[x, y] != biomeIndex)
                            continue;

                        var noiseValue = Mathf.PerlinNoise(x * workingXScale, y * workingYScale) * 2f - 1f;
                        var newHeight = heightMap[x, y] + noiseValue * workingHeightDelta / heightMapScale.y;

                        heightMap[x, y] = Mathf.Lerp(heightMap[x, y], newHeight, strength);
                    }
                }

                workingXScale *= xScaleVariationPerPass;
                workingYScale *= yScaleVariationPerPass;
                workingHeightDelta *= heightDeltaVariationPerPass;
            }
        }
    }
}