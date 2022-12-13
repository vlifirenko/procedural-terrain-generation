using System.Collections.Generic;
using PTG.Model.Config;
using UnityEngine;

namespace PTG
{
    public class ProcGenManager : MonoBehaviour
    {
        [SerializeField] private ProcGenConfig config;
        [SerializeField] private Terrain terrain;

#if UNITY_EDITOR
        private byte[,] _biomeMap;
        private float[,] _biomeStrengths;
#endif

#if UNITY_EDITOR
        public void RegenerateWorld()
        {
            // cache the map resolution
            var mapResolution = terrain.terrainData.heightmapResolution;

            PerformBiomeGeneration(mapResolution);
        }

        private void PerformBiomeGeneration(int mapResolution)
        {
            // allocate the biome map and strength map
            _biomeMap = new byte[mapResolution, mapResolution];
            _biomeStrengths = new float[mapResolution, mapResolution];

            // setup space for the seed points
            var numSeedPoints = Mathf.FloorToInt(mapResolution * mapResolution * config.biomeSeedPointDensity);
            var biomesToSpawn = new List<byte>(numSeedPoints);

            // populate the biomes to spawn based on weightings
            var totalBiomeWeight = config.TotalWeight;
            for (var biomeIndex = 0; biomeIndex < config.NumBiomes; biomeIndex++)
            {
                var numEntries = Mathf.RoundToInt(numSeedPoints * config.biomes[biomeIndex].weight / totalBiomeWeight);
                Debug.Log($"Will spawn {numEntries} seed points for {config.biomes[biomeIndex].biome.Name}");

                for (var entryIndex = 0; entryIndex < numEntries; entryIndex++)
                    biomesToSpawn.Add((byte) biomeIndex);
            }
        }
#endif
    }
}