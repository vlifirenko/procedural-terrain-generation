using System.Collections.Generic;
using System.Linq;
using PTG.HeightMapModifiers;
using PTG.Model.Config;
using UnityEngine;
#if UNITY_EDITOR
using PTG.Model;
using PTG.ObjectPlacers;
using PTG.TexturePainters;
using UnityEditor;
using UnityEngine.SceneManagement;

#endif

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
        private float[,] _slopeMap;

        private readonly Vector2Int[] _neighbourOffsets =
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

        private Dictionary<TextureConfig, int> _biomeTextureToTerrainLayerIndex = new Dictionary<TextureConfig, int>();
#endif

#if UNITY_EDITOR

        public void RegenerateTextures()
        {
            Perform_LayerSetup();
        }

        public void RegenerateWorld()
        {
            // cache the map resolution
            var mapResolution = terrain.terrainData.heightmapResolution;
            var alphaMapResolution = terrain.terrainData.alphamapResolution;
            
            for (var i = transform.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(transform.GetChild(i).gameObject);

            Perform_GenerateTextureMapping();

            Perform_BiomeGeneration_LowResolution((int) config.biomeMapBaseResolution);

            Perform_BiomeGeneration_HighResolution((int) config.biomeMapBaseResolution, mapResolution);

            Perform_HeightMapModification(mapResolution, alphaMapResolution);

            Perform_TerrainPainting(mapResolution, alphaMapResolution);

            Perform_ObjectPlacement(mapResolution, alphaMapResolution);
        }

        public int GetLayerForTexture(TextureConfig textureConfig) => _biomeTextureToTerrainLayerIndex[textureConfig];

        private void Perform_GenerateTextureMapping()
        {
            _biomeTextureToTerrainLayerIndex.Clear();

            var allTextures = new List<TextureConfig>();
            foreach (var biomeMetaData in config.biomes)
            {
                var biomeTextures = biomeMetaData.biome.RetrieveTextures();
                if (biomeTextures == null || biomeTextures.Count == 0)
                    continue;

                allTextures.AddRange(biomeTextures);
            }

            if (config.paintingPostProcessingModifier != null)
            {
                var allPainters = config.paintingPostProcessingModifier.GetComponents<BaseTexturePainter>();
                foreach (var painter in allPainters)
                {
                    var painterTextures = painter.RetrieveTextures();

                    if (painterTextures == null || painterTextures.Count == 0)
                        continue;

                    allTextures.AddRange(painterTextures);
                }
            }

            allTextures = allTextures.Distinct().ToList();

            var layerIndex = 0;

            foreach (var textureConfig in allTextures)
            {
                _biomeTextureToTerrainLayerIndex[textureConfig] = layerIndex;
                layerIndex++;
            }
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

        private void Perform_HeightMapModification(int mapResolution, int alphaMapResolution)
        {
            var heightMap = terrain.terrainData.GetHeights(0, 0, mapResolution, mapResolution);

            if (config.InitialHeightModifier != null)
            {
                var modifiers = config.InitialHeightModifier.GetComponents<BaseMapHeightModifier>();

                foreach (var modifier in modifiers)
                    modifier.Execute(mapResolution, heightMap, terrain.terrainData.heightmapScale);
            }

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

            _slopeMap = new float[alphaMapResolution, alphaMapResolution];

            for (var y = 0; y < alphaMapResolution; y++)
            {
                for (var x = 0; x < alphaMapResolution; x++)
                {
                    _slopeMap[x, y] = terrain.terrainData.GetInterpolatedNormal(
                        (float) x / alphaMapResolution,
                        (float) y / alphaMapResolution).y;
                }
            }
        }

        private void Perform_TerrainPainting(int mapResolution, int alphaMapResolution)
        {
            var heightMap = terrain.terrainData.GetHeights(0, 0, mapResolution, mapResolution);
            var alphaMaps = terrain.terrainData.GetAlphamaps(0, 0, alphaMapResolution, alphaMapResolution);


            for (var y = 0; y < alphaMapResolution; y++)
            {
                for (var x = 0; x < alphaMapResolution; x++)
                {
                    for (var layerIndex = 0; layerIndex < terrain.terrainData.alphamapLayers; layerIndex++)
                    {
                        alphaMaps[x, y, layerIndex] = 0;
                    }
                }
            }

            for (var i = 0; i < config.NumBiomes; i++)
            {
                var biome = config.biomes[i].biome;
                if (biome.terrainPainter == null)
                    continue;

                var modifiers = biome.terrainPainter.GetComponents<BaseTexturePainter>();

                foreach (var modifier in modifiers)
                    modifier.Execute(this, mapResolution, heightMap, terrain.terrainData.heightmapScale, _slopeMap,
                        alphaMaps, alphaMapResolution, _biomeMap, i, biome);
            }

            if (config.paintingPostProcessingModifier != null)
            {
                var modifiers = config.paintingPostProcessingModifier.GetComponents<BaseTexturePainter>();

                foreach (var modifier in modifiers)
                    modifier.Execute(this, mapResolution, heightMap, terrain.terrainData.heightmapScale, _slopeMap,
                        alphaMaps, alphaMapResolution);
            }

            terrain.terrainData.SetAlphamaps(0, 0, alphaMaps);
        }

        private void Perform_ObjectPlacement(int mapResolution, int alphaMapResolution)
        {
            var heightMap = terrain.terrainData.GetHeights(0, 0, mapResolution, mapResolution);
            var alphaMaps = terrain.terrainData.GetAlphamaps(0, 0, alphaMapResolution, alphaMapResolution);

            for (var i = 0; i < config.NumBiomes; i++)
            {
                var biome = config.biomes[i].biome;
                if (biome.objectPlacer == null)
                    continue;

                var modifiers = biome.objectPlacer.GetComponents<BaseObjectPlacer>();

                foreach (var modifier in modifiers)
                    modifier.Execute(transform, mapResolution, heightMap, terrain.terrainData.heightmapScale, _slopeMap,
                        alphaMaps, alphaMapResolution, _biomeMap, i, biome);
            }
        }

        private void Perform_LayerSetup()
        {
            if (terrain.terrainData.terrainLayers != null && terrain.terrainData.terrainLayers.Length > 0)
            {
                Undo.RecordObject(terrain, "Clearing previous layers");

                var layersToDelete = new List<string>();
                foreach (var layer in terrain.terrainData.terrainLayers)
                {
                    if (layer == null)
                        continue;

                    layersToDelete.Add(AssetDatabase.GetAssetPath(layer.GetInstanceID()));
                }

                terrain.terrainData.terrainLayers = null;
                foreach (var layerFile in layersToDelete)
                {
                    if (string.IsNullOrEmpty(layerFile))
                        continue;

                    AssetDatabase.DeleteAsset(layerFile);
                }

                Undo.FlushUndoRecordObjects();
            }

            var scenePath = System.IO.Path.GetDirectoryName(SceneManager.GetActiveScene().path);

            Perform_GenerateTextureMapping();

            var numLayers = _biomeTextureToTerrainLayerIndex.Count;
            var newLayers = new List<TerrainLayer>();

            for (var i = 0; i < numLayers; i++)
                newLayers.Add(new TerrainLayer());

            foreach (var entry in _biomeTextureToTerrainLayerIndex)
            {
                var textureConfig = entry.Key;
                var textureLayerIndex = entry.Value;
                var textureLayer = newLayers[textureLayerIndex];

                textureLayer.diffuseTexture = textureConfig.diffuse;
                textureLayer.normalMapTexture = textureConfig.normal;
                
                var layerPath = System.IO.Path.Combine(scenePath, $"Layer_{textureLayerIndex}");
                AssetDatabase.CreateAsset(textureLayer, layerPath);
            }

            Undo.RecordObject(terrain.terrainData, "Updating terrain layers");
            terrain.terrainData.terrainLayers = newLayers.ToArray();
        }
#endif
    }
}