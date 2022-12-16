using PTG.Model.Config;
using UnityEngine;

namespace PTG.HeightMapModifiers
{
    public class HeightMapModifier_Smooth : BaseMapHeightModifier
    {
        [SerializeField] private int smoothingKernelSize = 5;

        public override void Execute(int mapResolution, float[,] heightMap, Vector3 heightmapScale, byte[,] biomeMap = null,
            int biomeIndex = -1, BiomeConfig biome = null)
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

                    for (var yDelta = -smoothingKernelSize; yDelta <= smoothingKernelSize; yDelta++)
                    {
                        var workingY = y + yDelta;
                        if (workingY < 0 || workingY >= mapResolution)
                            continue;

                        for (var xDelta = -smoothingKernelSize; xDelta <= smoothingKernelSize; xDelta++)
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