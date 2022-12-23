using System.Collections.Generic;
using PTG.Model;
using PTG.Model.Config;
using UnityEngine;

namespace PTG.HeightMapModifiers
{
    public class HeightMapModifier_Features : BaseMapHeightModifier
    {
        [SerializeField] private int smoothingKernelSize = 5;
        [SerializeField] private List<FeatureConfig> features;

        public override void Execute(int mapResolution, float[,] heightMap, Vector3 heightMapScale, byte[,] biomeMap = null,
            int biomeIndex = -1, BiomeConfig biome = null)
        {
            foreach (var feature in features)
            {
                for (var i = 0; i < feature.numToSpawn; i++)
                {
                    var spawnX = Random.Range(feature.radius, mapResolution - feature.radius);
                    var spawnY = Random.Range(feature.radius, mapResolution - feature.radius);

                    Debug.Log($"Spawned feature at {spawnX},{spawnY}");

                    SpawnFeature(feature, spawnX, spawnY, mapResolution, heightMap, heightMapScale);
                }
            }
        }

        private void SpawnFeature(FeatureConfig feature, in int spawnX, in int spawnY, in int mapResolution, float[,] heightMap,
            Vector3 heightMapScale)
        {
            var averageHeight = 0f;
            var numHeightSamples = 0;

            for (var y = -feature.radius; y <= feature.radius; y++)
            {
                for (var x = -feature.radius; x < feature.radius; x++)
                {
                    averageHeight += heightMap[x + spawnX, y + spawnY];
                    numHeightSamples++;
                }
            }

            averageHeight /= numHeightSamples;

            var targetHeight = averageHeight + feature.height / heightMapScale.y;

            for (var y = -feature.radius; y <= feature.radius; y++)
            {
                var workingY = y + spawnY;
                var textureY = Mathf.Clamp01((y + feature.radius) / (feature.radius * 2f));
                for (var x = -feature.radius; x < feature.radius; x++)
                {
                    var workingX = x + spawnX;
                    var textureX = Mathf.Clamp01((x + feature.radius) / (feature.radius * 2f));

                    var pixelColor = feature.heightMap.GetPixelBilinear(textureX, textureY);
                    var workingStrength = pixelColor.r;

                    heightMap[workingX, workingY] = Mathf.Lerp(heightMap[workingX, workingY], targetHeight, workingStrength);
                }
            }
        }
    }
}