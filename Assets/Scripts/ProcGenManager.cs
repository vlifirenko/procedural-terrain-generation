using System.Collections.Generic;
using PTG.HeightMapModifiers;
using PTG.Model.Config;
using Unity.VisualScripting;
using UnityEngine;

namespace PTG
{
    public class ProcGenManager : MonoBehaviour
    {
        [SerializeField] private ProcGenConfig config;
        [SerializeField] private Terrain terrain;

#if UNITY_EDITOR
        private byte[,] _biomeMap_LowResolution;
        private float[,] _biomeStrengths_LowResolution;

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

            Perform_BiomeGeneration_LowResolution((int) config.biomeMapBaseResolution);

            Perform_BiomeGeneration_HighResolution((int) config.biomeMapBaseResolution, mapResolution);

            Perform_HeightMapModification(mapResolution);
        }

        private void Perform_BiomeGeneration_LowResolution(int mapResolution)
        {
            // allocate the biome map and strength map
            _biomeMap_LowResolution = new byte[mapResolution, mapResolution];
            _biomeStrengths_LowResolution = new float[mapResolution, mapResolution];

            // setup space for the seed points
            var numSeedPoints = Mathf.FloorToInt(mapResolution * mapResolution * config.biomeSeedPointDensity);
            var biomesToSpawn = new List<byte>(numSeedPoints);

            // populate the biomes to spawn based on weightings
            var totalBiomeWeight = config.TotalWeight;
            for (var biomeIndex = 0; biomeIndex < config.NumBiomes; biomeIndex++)
            {
                var numEntries = Mathf.RoundToInt(numSeedPoints * config.biomes[biomeIndex].weight / totalBiomeWeight);
                //Debug.Log($"Will spawn {numEntries} seed points for {config.biomes[biomeIndex].biome.Name}");

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

                Perform_SpawnIndividualBiome(biomeIndex, mapResolution);
            }

            var biomeTextureMap = new Texture2D(mapResolution, mapResolution, TextureFormat.RGB24, false);
            for (var y = 0; y < mapResolution; y++)
            {
                for (var x = 0; x < mapResolution; x++)
                {
                    var hue = _biomeMap_LowResolution[x, y] / (float) config.NumBiomes;
                    biomeTextureMap.SetPixel(x, y, Color.HSVToRGB(hue, 0.75f, 0.75f));
                }
            }

            biomeTextureMap.Apply();
            System.IO.File.WriteAllBytes("BiomeMap_LowRes.png", biomeTextureMap.EncodeToPNG());
        }

        private void Perform_BiomeGeneration_HighResolution(int lowResMapSize, int highResMapSize)
        {
            // allocate the biome map and strength map
            _biomeMap = new byte[highResMapSize, highResMapSize];
            _biomeStrengths = new float[highResMapSize, highResMapSize];

            var mapScale = (float) lowResMapSize / highResMapSize;

            for (var y = 0; y < highResMapSize; y++)
            {
                var lowResY = Mathf.FloorToInt(y * mapScale);
                var yFraction = y * mapScale - lowResY;

                for (var x = 0; x < highResMapSize; x++)
                {
                    var lowResX = Mathf.FloorToInt(x * mapScale);
                    var xFraction = x * mapScale - lowResX;

                    _biomeMap[x, y] = CalculateHighResBiomeIndex(lowResMapSize, lowResX, lowResY, xFraction, yFraction);
                    //_biomeMap[x, y] = _biomeMap_LowResolution[lowResX, lowResY];
                }
            }

            var biomeTextureMap = new Texture2D(highResMapSize, highResMapSize, TextureFormat.RGB24, false);
            for (var y = 0; y < highResMapSize; y++)
            {
                for (var x = 0; x < highResMapSize; x++)
                {
                    var hue = _biomeMap[x, y] / (float) config.NumBiomes;
                    biomeTextureMap.SetPixel(x, y, Color.HSVToRGB(hue, 0.75f, 0.75f));
                }
            }

            biomeTextureMap.Apply();
            System.IO.File.WriteAllBytes("BiomeMap_HighRes.png", biomeTextureMap.EncodeToPNG());
        }

        private void Perform_SpawnIndividualBiome(byte biomeIndex, int mapResolution)
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

                _biomeMap_LowResolution[workingLocation.x, workingLocation.y] = biomeIndex;
                visited[workingLocation.x, workingLocation.y] = true;
                _biomeStrengths_LowResolution[workingLocation.x, workingLocation.y] =
                    targetIntensity[workingLocation.x, workingLocation.y];

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

        private byte CalculateHighResBiomeIndex(int lowResMapSize, int lowResX, int lowResY, float fractionX, float fractionY)
        {
            var a = _biomeMap_LowResolution[lowResX, lowResY];
            var b = lowResX + 1 < lowResMapSize ? _biomeMap_LowResolution[lowResX + 1, lowResY] : a;
            var c = lowResY + 1 < lowResMapSize ? _biomeMap_LowResolution[lowResX, lowResY + 1] : a;
            int d;

            if (lowResX + 1 >= lowResMapSize)
                d = c;
            else if (lowResY + 1 >= lowResMapSize)
                d = b;
            else
                d = _biomeMap_LowResolution[lowResX + 1, lowResY + 1];

            var filteredIndex = a * (1 - fractionX) * (1 - fractionY) + b * fractionX * (1 - fractionY) *
                c * fractionY * (1 - fractionX) + d * fractionX * fractionY;

            var candidateBiomes = new float[] {a, b, c, d};
            var bestBiome = -1f;
            var bestDelta = float.MaxValue;

            for (var i = 0; i < candidateBiomes.Length; i++)
            {
                var delta = Mathf.Abs(filteredIndex - candidateBiomes[i]);
                if (delta < bestDelta)
                {
                    bestDelta = delta;
                    bestBiome = candidateBiomes[i];
                }
            }

            return (byte) Mathf.RoundToInt(bestBiome);
        }

        private void Perform_HeightMapModification(in int mapResolution)
        {
            var heightMap = terrain.terrainData.GetHeights(0, 0, mapResolution, mapResolution);

            if (config.InitialHeightModifier != null)
            {
                var modifiers = config.InitialHeightModifier.GetComponents<BaseMapHeightModifier>();

                foreach (var modifier in modifiers)
                    modifier.Execute(mapResolution, heightMap, terrain.terrainData.heightmapScale);
            }

            //foreach (var biome in config.biomes)
            for (var i = 0; i < config.NumBiomes; i++)
            {
                var biome = config.biomes[i].biome;
                if (biome.heightModifier == null)
                    continue;
                
                var modifiers = biome.heightModifier.GetComponents<BaseMapHeightModifier>();

                foreach (var modifier in modifiers)
                    modifier.Execute(mapResolution, heightMap, terrain.terrainData.heightmapScale, _biomeMap, i, biome);
            }

            if (config.HeightPostProcessingModifier != null)
            {
                var modifiers = config.HeightPostProcessingModifier.GetComponents<BaseMapHeightModifier>();

                foreach (var modifier in modifiers)
                    modifier.Execute(mapResolution, heightMap, terrain.terrainData.heightmapScale);
            }

            terrain.terrainData.SetHeights(0, 0, heightMap);
        }

#endif
    }
}