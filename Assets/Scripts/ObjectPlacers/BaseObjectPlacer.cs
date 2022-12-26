using System;
using System.Collections.Generic;
using System.Linq;
using PTG.Model;
using PTG.Model.Config;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace PTG.ObjectPlacers
{
    public class BaseObjectPlacer : MonoBehaviour
    {
        [SerializeField] protected List<PlaceableObjectConfig> objects;
        [SerializeField] protected float targetDensity = 0.1f;
        [SerializeField] protected int maxSpawnCount = 1000;
        [SerializeField] protected int maxInvalidLocationSkips = 10;
        [SerializeField] protected float maxPositionJitter = 0.1f;

        public virtual void Execute(ProcGenConfig globalConfig, Transform objectRoot, int mapResolution, float[,] heightMap,
            Vector3 heightMapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null,
            int biomeIndex = -1, BiomeConfig biome = null)
        {
            foreach (var config in objects)
            {
                if (!config.canGoInWater && !config.canGoAboveWater)
                    throw new InvalidOperationException($"Object placer forbids both in and ou of water. Cannot run!");
            }

            var weightSum = objects.Sum(config => config.weighting);
            foreach (var config in objects)
                config.NormalisedWeighting = config.weighting / weightSum;
        }

        protected List<Vector3> GetAllLocationsForBiome(ProcGenConfig globalConfig, int mapResolution, float[,] heightMap,
            Vector3 heightMapScale, byte[,] biomeMap, int biomeIndex)
        {
            var locations = new List<Vector3>(mapResolution * mapResolution / 10);

            for (var y = 0; y < mapResolution; y++)
            {
                for (var x = 0; x < mapResolution; x++)
                {
                    if (biomeMap[x, y] != biomeIndex)
                        continue;

                    var height = heightMap[x, y] * heightMapScale.y;

                    locations.Add(new Vector3(
                        y * heightMapScale.z,
                        height,
                        x * heightMapScale.x));
                }
            }

            return locations;
        }

        protected virtual void ExecuteSimpleSpawning(ProcGenConfig globalConfig, Transform objectRoot,
            List<Vector3> candidateLocations)
        {
            foreach (var spawnConfig in objects)
            {
                var prefab = spawnConfig.prefabs[Random.Range(0, spawnConfig.prefabs.Count)];
                var baseSpawnCount = Math.Min(maxSpawnCount, candidateLocations.Count * targetDensity);
                var numToSpawn = Mathf.FloorToInt(spawnConfig.NormalisedWeighting * baseSpawnCount);

                var skipCount = 0;
                var numPlaced = 0;
                for (var i = 0; i < numToSpawn; i++)
                {
                    var randomLocationIndex = Random.Range(0, candidateLocations.Count);
                    var spawnLocation = candidateLocations[randomLocationIndex];
                    var isValid = true;

                    if (spawnLocation.y < globalConfig.waterHeight && !spawnConfig.canGoInWater)
                        isValid = false;
                    if (spawnLocation.y >= globalConfig.waterHeight && !spawnConfig.canGoAboveWater)
                        isValid = false;

                    if (spawnConfig.hasHeightLimits && (spawnLocation.y < spawnConfig.minHeightToSpawn ||
                                                        spawnLocation.y >= spawnConfig.maxHeightToSpawn))
                        isValid = false;

                    if (!isValid)
                    {
                        skipCount++;
                        i--;

                        if (skipCount >= maxInvalidLocationSkips)
                            break;

                        continue;
                    }

                    skipCount = 0;
                    numPlaced++;

                    candidateLocations.RemoveAt(randomLocationIndex);

                    SpawnObject(prefab, spawnLocation, objectRoot);
                }

                Debug.Log($"Placed {numPlaced} objects out of {numToSpawn}");
            }
        }

        protected virtual void SpawnObject(GameObject prefab, Vector3 spawnLocation, Transform objectRoot)
        {
            var spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            var positionOffset = new Vector3(
                Random.Range(-maxPositionJitter, maxPositionJitter),
                0,
                Random.Range(-maxPositionJitter, maxPositionJitter));

#if UNITY_EDITOR
            if (Application.isPlaying)
                Instantiate(prefab, spawnLocation + positionOffset, spawnRotation, objectRoot);
            else
            {
                var spawnedGO = PrefabUtility.InstantiatePrefab(prefab, objectRoot) as GameObject;
                spawnedGO.transform.position = spawnLocation + positionOffset;
                spawnedGO.transform.rotation = spawnRotation;
                Undo.RegisterCreatedObjectUndo(spawnedGO, "Placed object");
            }
#else
            Instantiate(prefab, spawnLocation + positionOffset, spawnRotation, objectRoot);
#endif
        }
    }
}