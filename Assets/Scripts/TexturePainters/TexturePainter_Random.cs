﻿using System.Collections.Generic;
using PTG.Model.Config;
using UnityEngine;

namespace PTG.TexturePainters
{
    public class TexturePainter_Random : BaseTexturePainter
    {
        [SerializeField] private List<string> textureIDs;

        public override void Execute(ProcGenManager procGenManager, int mapResolution, float[,] heightMap, Vector3 heightmapScale,
            float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1,
            BiomeConfig biome = null)
        {
            for (var y = 0; y < alphaMapResolution; y++)
            {
                var heightMapY = Mathf.FloorToInt((float) y * mapResolution / alphaMapResolution);

                for (var x = 0; x < alphaMapResolution; x++)
                {
                    var heightMapX = Mathf.FloorToInt((float) x * mapResolution / alphaMapResolution);

                    // skip if we have a biome and this is not our biome
                    if (biomeIndex >= 0 && biomeMap[heightMapX, heightMapY] != biomeIndex)
                        continue;

                    var randomTexture = textureIDs[Random.Range(0, textureIDs.Count)];
                    var terrainLayer = procGenManager.GetLayerForTexture(randomTexture);

                    alphaMaps[x, y, terrainLayer] = strength;
                }
            }
        }
    }
}