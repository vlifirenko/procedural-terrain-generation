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
        [SerializeField] private float targetDensity = 0.1f;
        [SerializeField] private int maxSpawnCount = 1000;
        [SerializeField] private Vector2 noiseScale = new Vector2(1f / 128f, 1f / 128f);
        [SerializeField] private float noiseThreshold = 0.5f;
        [SerializeField] private GameObject prefab;

        public override void Execute(ProcGenConfig globalConfig, Transform objectRoot, int mapResolution, float[,] heightMap,
            Vector3 heightmapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null,
            int biomeIndex = -1, BiomeConfig biome = null)
        {
            var candidateLocations =
                GetFilteredLocationsForBiome(globalConfig, mapResolution, heightMap, heightmapScale, biomeMap, biomeIndex);

            var numToSpawn = Mathf.FloorToInt(Mathf.Min(maxSpawnCount, candidateLocations.Count * targetDensity));
            for (var index = 0; index < numToSpawn; ++index)
            {
                var randomLocationIndex = Random.Range(0, candidateLocations.Count);
                var spawnLocation = candidateLocations[randomLocationIndex];
                candidateLocations.RemoveAt(randomLocationIndex);

#if UNITY_EDITOR
                if (Application.isPlaying)
                    Instantiate(prefab, spawnLocation, Quaternion.identity, objectRoot);
                else
                {
                    var spawnedGO = PrefabUtility.InstantiatePrefab(prefab, objectRoot) as GameObject;
                    spawnedGO.transform.position = spawnLocation;
                    Undo.RegisterCreatedObjectUndo(spawnedGO, "Placed object");
                }
#else
            Instantiate(prefab, spawnLocation, Quaternion.identity, objectRoot);
#endif
            }
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

                    if (height < globalConfig.waterHeight && !canGoInWater)
                        continue;
                    if (height >= globalConfig.waterHeight && !canGoAboveWater)
                        continue;

                    if (hasHeightLimits && (height < minHeightToSpawn || height >= maxHeightToSpawn))
                        continue;

                    locations.Add(new Vector3(y * heightmapScale.z, height, x * heightmapScale.x));
                }
            }

            return locations;
        }
    }
}