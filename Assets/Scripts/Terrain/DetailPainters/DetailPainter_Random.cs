using System;
using System.Collections.Generic;
using PTG.Model;
using PTG.Model.Config;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PTG.Terrain.DetailPainters
{
    public class DetailPainter_Random : BaseDetailPainter
    {
        [SerializeField] private List<RandomDetailPainterConfig> paintingConfigs = new List<RandomDetailPainterConfig>()
        {
            new RandomDetailPainterConfig()
        };

        [NonSerialized] private List<TerrainDetailConfig> _cachedTerrainDetails;

        public override void Execute(ProcGenManager procGenManager, int mapResolution, float[,] heightMap,
            Vector3 terrainDataHeightmapScale, float[,] slopeMap,
            float[,,] alphaMaps, int alphaMapResolution, List<int[,]> detailLayerMaps, int detailMapResolution,
            int maxDetailItems,
            byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfig biome = null)
        {
            for (var y = 0; y < detailMapResolution; y++)
            {
                var heightMapY = Mathf.FloorToInt((float) y * mapResolution / detailMapResolution);

                for (var x = 0; x < detailMapResolution; x++)
                {
                    var heightMapX = Mathf.FloorToInt((float) x * mapResolution / detailMapResolution);

                    // skip if we have a biome and this is not our biome
                    if (biomeIndex >= 0 && biomeMap[heightMapX, heightMapY] != biomeIndex)
                        continue;

                    foreach (var config in paintingConfigs)
                    {
                        var noiseValue = Mathf.PerlinNoise(x * config.noiseScale, y * config.noiseScale);
                        if (Random.Range(0f, 1f) >= noiseValue)
                        {
                            var layer = procGenManager.GetDetailLayerForTerrainDetail(config.detailToPaint);
                            detailLayerMaps[layer][x, y] = Mathf.FloorToInt(strength * config.intensityModifier * maxDetailItems);
                        }
                    }
                }
            }
        }

        public override List<TerrainDetailConfig> RetrieveTerrainDetails()
        {
            if (_cachedTerrainDetails == null)
            {
                _cachedTerrainDetails = new List<TerrainDetailConfig>();
                foreach (var config in paintingConfigs)
                    _cachedTerrainDetails.Add(config.detailToPaint);
            }

            return _cachedTerrainDetails;
        }
    }
}