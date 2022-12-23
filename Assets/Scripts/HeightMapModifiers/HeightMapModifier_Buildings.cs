using System.Collections.Generic;
using PTG.Model;
using PTG.Model.Config;
using UnityEngine;

namespace PTG.HeightMapModifiers
{
    public class HeightMapModifier_Buildings : BaseMapHeightModifier
    {
        [SerializeField] private int smoothingKernelSize = 5;
        [SerializeField] private List<BuildingConfig> buildings;

        public override void Execute(int mapResolution, float[,] heightMap, Vector3 heightMapScale, byte[,] biomeMap = null,
            int biomeIndex = -1, BiomeConfig biome = null)
        {
            var buildingRoot = FindObjectOfType<ProcGenManager>().transform;
            
            foreach (var building in buildings)
            {
                for (var i = 0; i < building.numToSpawn; i++)
                {
                    var spawnX = Random.Range(building.radius, mapResolution - building.radius);
                    var spawnY = Random.Range(building.radius, mapResolution - building.radius);

                    SpawnBuilding(building, spawnX, spawnY, mapResolution, heightMap, heightMapScale, buildingRoot);
                }
            }
        }

        private void SpawnBuilding(BuildingConfig building, in int spawnX, in int spawnY, in int mapResolution,
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

            Instantiate(building.prefab, buildingLocation, Quaternion.identity, buildingRoot);
        }
    }
}