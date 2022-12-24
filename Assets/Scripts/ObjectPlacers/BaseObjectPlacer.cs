using System.Collections.Generic;
using PTG.Model.Config;
using UnityEngine;

namespace PTG.ObjectPlacers
{
    public class BaseObjectPlacer : MonoBehaviour
    {
        [SerializeField] protected bool hasHeightLimits;
        [SerializeField] protected float minHeightToSpawn;
        [SerializeField] protected float maxHeightToSpawn;
        [SerializeField] protected bool canGoInWater;
        [SerializeField] protected bool canGoAboveWater = true;

        public virtual void Execute(ProcGenConfig globalConfig, Transform objectRoot, int mapResolution, float[,] heightMap,
            Vector3 heightMapScale, float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null,
            int biomeIndex = -1, BiomeConfig biome = null)
            => Debug.LogError($"No implementation of Execute function for {gameObject.name}");

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

                    if (height < globalConfig.waterHeight && !canGoInWater)
                        continue;
                    if (height >= globalConfig.waterHeight && !canGoAboveWater)
                        continue;

                    if (hasHeightLimits && (height < minHeightToSpawn || height >= maxHeightToSpawn))
                        continue;

                    locations.Add(new Vector3(
                        y * heightMapScale.z,
                        height,
                        x * heightMapScale.x));
                }
            }

            return locations;
        }
    }
}