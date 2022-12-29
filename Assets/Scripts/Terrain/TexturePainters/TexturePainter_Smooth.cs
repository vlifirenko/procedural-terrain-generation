using PTG.Model.Config;
using UnityEngine;

namespace PTG.Terrain.TexturePainters
{
    public class TexturePainter_Smooth : BaseTexturePainter
    {
        [SerializeField] private int smoothingKernelSize = 5;

        public override void Execute(ProcGenManager procGenManager, int mapResolution, float[,] heightMap, Vector3 heightmapScale,
            float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1,
            BiomeConfig biome = null)
        {
            if (biomeMap != null)
            {
                Debug.LogError($"TexturePainter_Smooth is not supported as a per biome modifier [{gameObject.name}]");
                return;
            }

            for (var layer = 0; layer < alphaMaps.GetLength(2); layer++)
            {
                var smoothedAlphaMap = new float[alphaMapResolution, alphaMapResolution];

                for (var y = 0; y < alphaMapResolution; y++)
                {
                    for (var x = 0; x < alphaMapResolution; x++)
                    {
                        var alphaSum = 0f;
                        var numValues = 0;

                        for (var yDelta = -smoothingKernelSize; yDelta <= smoothingKernelSize; yDelta++)
                        {
                            var workingY = y + yDelta;
                            if (workingY < 0 || workingY >= alphaMapResolution)
                                continue;

                            for (var xDelta = -smoothingKernelSize; xDelta <= smoothingKernelSize; xDelta++)
                            {
                                var workingX = x + xDelta;
                                if (workingX < 0 || workingX >= alphaMapResolution)
                                    continue;

                                alphaSum += alphaMaps[workingX, workingY, layer];
                                numValues++;
                            }
                        }

                        smoothedAlphaMap[x, y] = alphaSum / numValues;
                    }
                }

                for (var y = 0; y < alphaMapResolution; y++)
                {
                    for (var x = 0; x < alphaMapResolution; x++)
                    {
                        alphaMaps[x, y, layer] = Mathf.Lerp(alphaMaps[x, y, layer], smoothedAlphaMap[x, y], strength);
                    }
                }
            }
        }
    }
}