using System;
using PTG.Model.Config;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PTG.ObjectPlacers
{
    public class ObjectPlacer_Random : BaseObjectPlacer
    {
        [SerializeField] private float targetDensity = 0.1f;
        [SerializeField] private int maxSpawnCount = 1000;
        [SerializeField] private GameObject prefab;

        public override void Execute(ProcGenConfig globalConfig, Transform objectRoot, int mapResolution, float[,] heightMap,
            Vector3 heightMapScale,
            float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1,
            BiomeConfig biome = null)
        {
            var candidateLocations = GetAllLocationsForBiome(globalConfig, mapResolution, heightMap, heightMapScale, biomeMap, biomeIndex);

            var numToSpawn = Mathf.FloorToInt(Math.Min(maxSpawnCount, candidateLocations.Count * targetDensity));
            for (var i = 0; i < numToSpawn; i++)
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
            Instantiate(Prefab, spawnLocation, Quaternion.identity, objectRoot);
#endif
            }
        }
    }
}