using PTG.Model.Config;
using UnityEngine;

namespace PTG.HeightMapModifiers
{
    public class HeightMapModifier_Smooth : BaseMapHeightModifier
    {
        [SerializeField] private int smoothingKernelSize = 5;
        [SerializeField] private bool useAdaptiveKernel;
        [SerializeField, Range(0f, 1f)] private float maxHeightThreshold = 0.5f;
        [SerializeField] private int minKernelSize = 2;
        [SerializeField] private int maxKernelSize = 7;

        public override void Execute(ProcGenConfig globalConfig, int mapResolution, float[,] heightMap, Vector3 heightMapScale,
            byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfig biome = null)
        {
            if (biomeMap != null)
            {
                Debug.LogError($"HeightMapModifier_Smooth is not supported as a per biome modifier [{gameObject.name}]");
                return;
            }

            var smoothedHeights = new float[mapResolution, mapResolution];

            for (var y = 0; y < mapResolution; y++)
            {
                for (var x = 0; x < mapResolution; x++)
                {
                    var heightSum = 0f;
                    var numValues = 0;

                    var kernelSize = smoothingKernelSize;
                    if (useAdaptiveKernel)
                    {
                        kernelSize = Mathf.RoundToInt(Mathf.Lerp(minKernelSize, maxKernelSize,
                            heightMap[x, y] / maxHeightThreshold));
                    }

                    for (var yDelta = -kernelSize; yDelta <= kernelSize; yDelta++)
                    {
                        var workingY = y + yDelta;
                        if (workingY < 0 || workingY >= mapResolution)
                            continue;

                        for (var xDelta = -kernelSize; xDelta <= kernelSize; xDelta++)
                        {
                            var workingX = x + xDelta;
                            if (workingX < 0 || workingX >= mapResolution)
                                continue;

                            heightSum += heightMap[workingX, workingY];
                            numValues++;
                        }
                    }

                    smoothedHeights[x, y] = heightSum / numValues;
                }
            }

            for (var y = 0; y < mapResolution; y++)
            {
                for (var x = 0; x < mapResolution; x++)
                {
                    heightMap[x, y] = Mathf.Lerp(heightMap[x, y], smoothedHeights[x, y], strength);
                }
            }
        }
    }
}