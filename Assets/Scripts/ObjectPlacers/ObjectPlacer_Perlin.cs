using System.Collections.Generic;
using PTG.Model.Config;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

#endif

namespace PTG.ObjectPlacers
{
    public class ObjectPlacer_Perlin : BaseObjectPlacer
    {
        [SerializeField] private Vector2 noiseScale = new Vector2(1f / 128f, 1f / 128f);
        [SerializeField] private float noiseThreshold = 0.5f;

        public override void Execute(ProcGenConfig globalConfig, Transform objectRoot, int mapResolution, float[,] heightMap,
            Vector3 heightMapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null,
            int biomeIndex = -1, BiomeConfig biome = null)
        {
            base.Execute(globalConfig, objectRoot, mapResolution, heightMap, heightMapScale, slopeMap, alphaMaps,
                alphaMapResolution, biomeMap, biomeIndex, biome);

            var candidateLocations =
                GetFilteredLocationsForBiome(globalConfig, mapResolution, heightMap, heightMapScale, biomeMap, biomeIndex);

            ExecuteSimpleSpawning(globalConfig, objectRoot, candidateLocations);
        }

        private List<Vector3> GetFilteredLocationsForBiome(ProcGenConfig globalConfig, int mapResolution, float[,] heightMap,
            Vector3 heightmapScale, byte[,] biomeMap, int biomeIndex)
        {
            var locations = new List<Vector3>(mapResolution * mapResolution / 10);

            for (var y = 0; y < mapResolution; ++y)
            {
                for (var x = 0; x < mapResolution; ++x)
                {
                    if (biomeMap[x, y] != biomeIndex)
                        continue;

                    var noiseValue = Mathf.PerlinNoise(x * noiseScale.x, y * noiseScale.y);

                    if (noiseValue < noiseThreshold)
                        continue;

                    var height = heightMap[x, y] * heightmapScale.y;

                    locations.Add(new Vector3(y * heightmapScale.z, height, x * heightmapScale.x));
                }
            }

            return locations;
        }
    }
}