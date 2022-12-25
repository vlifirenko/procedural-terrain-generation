using System.Collections.Generic;
using PTG.Model;
using PTG.Model.Config;
using UnityEditor;
using UnityEngine;

namespace PTG.HeightMapModifiers
{
    public class HeightMapModifier_Buildings : BaseMapHeightModifier
    {
        [SerializeField] private List<BuildingConfig> buildings;

        public override void Execute(ProcGenConfig globalConfig, int mapResolution, float[,] heightMap, Vector3 heightMapScale,
            byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfig biome = null)
        {
            var buildingRoot = FindObjectOfType<ProcGenManager>().transform;

            foreach (var building in buildings)
            {
                var spawnLocations =
                    GetSpawnLocationsForBuilding(globalConfig, mapResolution, heightMap, heightMapScale, building);

                for (var i = 0; i < building.numToSpawn && spawnLocations.Count > 0; i++)
                {
                    var spawnIndex = Random.Range(0, spawnLocations.Count);
                    var spawnPos = spawnLocations[spawnIndex];

                    spawnLocations.RemoveAt(spawnIndex);
                    SpawnBuilding(globalConfig, building, spawnPos.x, spawnPos.y, mapResolution, heightMap, heightMapScale,
                        buildingRoot);
                }
            }
        }

        private void SpawnBuilding(ProcGenConfig globalConfig, BuildingConfig building, int spawnX, int spawnY, int mapResolution,
            float[,] heightMap, Vector3 heightMapScale, Transform buildingRoot)
        {
            var averageHeight = 0f;
            var numHeightSamples = 0;

            for (var y = -building.radius; y <= building.radius; y++)
            {
                for (var x = -building.radius; x < building.radius; x++)
                {
                    averageHeight += heightMap[x + spawnX, y + spawnY];
                    numHeightSamples++;
                }
            }

            averageHeight /= numHeightSamples;

            var targetHeight = averageHeight;

            if (!building.canGoInWater)
                targetHeight = Mathf.Max(targetHeight, globalConfig.waterHeight / heightMapScale.y);
            if (building.hasHeightLimits)
                targetHeight = Mathf.Clamp(targetHeight, building.minHeightToSpawn / heightMapScale.y,
                    building.maxHeightToSpawn / heightMapScale.y);

            for (var y = -building.radius; y <= building.radius; y++)
            {
                var workingY = y + spawnY;
                var textureY = Mathf.Clamp01((y + building.radius) / (building.radius * 2f));
                for (var x = -building.radius; x < building.radius; x++)
                {
                    var workingX = x + spawnX;
                    var textureX = Mathf.Clamp01((x + building.radius) / (building.radius * 2f));

                    var pixelColor = building.heightMap.GetPixelBilinear(textureX, textureY);
                    var workingStrength = pixelColor.r;

                    heightMap[workingX, workingY] = Mathf.Lerp(heightMap[workingX, workingY], targetHeight, workingStrength);
                }
            }

            var buildingLocation = new Vector3(
                spawnY * heightMapScale.z,
                heightMap[spawnX, spawnY] * heightMapScale.y,
                spawnX * heightMapScale.x);

#if UNITY_EDITOR
            if (Application.isPlaying)
                Instantiate(building.prefab, buildingLocation, Quaternion.identity, buildingRoot);
            else
            {
                var spawnedGO = PrefabUtility.InstantiatePrefab(building.prefab, buildingRoot) as GameObject;
                spawnedGO.transform.position = buildingLocation;
                Undo.RegisterCreatedObjectUndo(spawnedGO, "Placed object");
            }
#else
            Instantiate(building.prefab, buildingLocation, Quaternion.identity, buildingRoot);
#endif
        }

        protected List<Vector2Int> GetSpawnLocationsForBuilding(ProcGenConfig globalConfig, int mapResolution, float[,] heightMap,
            Vector3 heightMapScale, BuildingConfig config)
        {
            var locations = new List<Vector2Int>(mapResolution * mapResolution / 10);

            for (var y = config.radius; y < mapResolution - config.radius; y += config.radius * 2)
            {
                for (var x = config.radius; x < mapResolution - config.radius; x += config.radius * 2)
                {
                    var height = heightMap[x, y] * heightMapScale.y;

                    if (height < globalConfig.waterHeight && !config.canGoInWater)
                        continue;
                    if (height >= globalConfig.waterHeight && !config.canGoAboveWater)
                        continue;

                    if (config.hasHeightLimits && (height < config.minHeightToSpawn || height >= config.maxHeightToSpawn))
                        continue;

                    locations.Add(new Vector2Int(x, y));
                }
            }

            return locations;
        }
    }
}