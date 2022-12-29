using System;
using System.Collections.Generic;
using PTG.Model;
using PTG.Model.Config;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PTG.Terrain.TexturePainters
{
    public class TexturePainter_Random : BaseTexturePainter
    {
        [SerializeField] private TextureConfig baseTexture;
        [SerializeField] private List<RandomPainterConfig> paintingConfigs;

        [NonSerialized] private List<TextureConfig> _cachedTextures;

        public override void Execute(ProcGenManager procGenManager, int mapResolution, float[,] heightMap, Vector3 heightmapScale,
            float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1,
            BiomeConfig biome = null)
        {
            var baseTextureLayer = procGenManager.GetLayerForTexture(baseTexture);

            for (var y = 0; y < alphaMapResolution; y++)
            {
                var heightMapY = Mathf.FloorToInt((float) y * mapResolution / alphaMapResolution);

                for (var x = 0; x < alphaMapResolution; x++)
                {
                    var heightMapX = Mathf.FloorToInt((float) x * mapResolution / alphaMapResolution);

                    // skip if we have a biome and this is not our biome
                    if (biomeIndex >= 0 && biomeMap[heightMapX, heightMapY] != biomeIndex)
                        continue;

                    foreach (var config in paintingConfigs)
                    {
                        var noiseValue = Mathf.PerlinNoise(x * config.noiseScale, y * config.noiseScale);
                        if (Random.Range(0f, 1f) >= noiseValue)
                        {
                            var layer = procGenManager.GetLayerForTexture(config.textureToPaint);
                            alphaMaps[x, y, layer] = strength * config.intensityModifier;
                        }
                    }

                    alphaMaps[x, y, baseTextureLayer] = strength;
                }
            }
        }

        public override List<TextureConfig> RetrieveTextures()
        {
            if (_cachedTextures == null)
            {
                _cachedTextures = new List<TextureConfig> {baseTexture};
                foreach (var config in paintingConfigs)
                    _cachedTextures.Add(config.textureToPaint);
            }

            return _cachedTextures;
        }
    }
}