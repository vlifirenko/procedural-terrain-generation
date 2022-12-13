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

        private readonly Vector2Int[] _neighbourOffsets = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(-1, -1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
        };
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

            // spawn the individual biomes
            while (biomesToSpawn.Count > 0)
            {
                // pick a random seed point
                var seedPointIndex = Random.Range(0, biomesToSpawn.Count);

                // extract the biome index
                var biomeIndex = biomesToSpawn[seedPointIndex];

                // remove seed point
                biomesToSpawn.RemoveAt(seedPointIndex);

                PerformSpawnIndividualBiome(biomeIndex, mapResolution);
            }

            var biomeTextureMap = new Texture2D(mapResolution, mapResolution, TextureFormat.RGB24, false);
            for (var y = 0; y < mapResolution; y++)
            {
                for (var x = 0; x < mapResolution; x++)
                {
                    var hue = _biomeMap[x, y] / (float) config.NumBiomes;
                    biomeTextureMap.SetPixel(x, y, Color.HSVToRGB(hue, 0.75f, 0.75f));
                }
            }

            biomeTextureMap.Apply();
            System.IO.File.WriteAllBytes("BiomeMap.png", biomeTextureMap.EncodeToPNG());
        }

        private void PerformSpawnIndividualBiome(byte biomeIndex, int mapResolution)
        {
            var biomeConfig = config.biomes[biomeIndex].biome;
            var spawnLocation = new Vector2Int(Random.Range(0, mapResolution), Random.Range(0, mapResolution));
            var startIntensity = Random.Range(biomeConfig.maxIntensity, biomeConfig.maxIntensity);
            var workingList = new Queue<Vector2Int>();
            var visited = new bool[mapResolution, mapResolution];
            var targetIntensity = new float[mapResolution, mapResolution];

            targetIntensity[spawnLocation.x, spawnLocation.y] = startIntensity;
            workingList.Enqueue(spawnLocation);

            while (workingList.Count > 0)
            {
                var workingLocation = workingList.Dequeue();

                _biomeMap[workingLocation.x, workingLocation.y] = biomeIndex;
                visited[workingLocation.x, workingLocation.y] = true;
                _biomeStrengths[workingLocation.x, workingLocation.y] = targetIntensity[workingLocation.x, workingLocation.y];

                for (var i = 0; i < _neighbourOffsets.Length; i++)
                {
                    var neighbourLocation = workingLocation + _neighbourOffsets[i];
                    if (neighbourLocation.x < 0 || neighbourLocation.y < 0 || neighbourLocation.x >= mapResolution ||
                        neighbourLocation.y >= mapResolution)
                        continue;

                    if (visited[neighbourLocation.x, neighbourLocation.y])
                        continue;

                    visited[neighbourLocation.x, neighbourLocation.y] = true;

                    var decayAmount = Random.Range(biomeConfig.minDecayRate, biomeConfig.maxDecayRate)
                                      * _neighbourOffsets[i].magnitude;
                    var neighbourStrength = targetIntensity[workingLocation.x, workingLocation.y] - decayAmount;

                    targetIntensity[neighbourLocation.x, neighbourLocation.y] = neighbourStrength;

                    if (neighbourStrength <= 0)
                        continue;

                    workingList.Enqueue(neighbourLocation);
                }
            }
        }
#endif
    }
}