using System.Collections.Generic;
using PTG.Model.Config;
using UnityEngine;

namespace PTG.TexturePainters
{
    public class TexturePainter_Random : BaseTexturePainter
    {
        [SerializeField] private List<string> textureIDs;

        public override void Execute(ProcGenManager procGenManager, int mapResolution, float[,] heightMap, Vector3 heightmapScale,
            float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfig biome = null)
        {
            for (var y = 0; y < alphaMapResolution; y++)
            {
                for (var x = 0; x < alphaMapResolution; x++)
                {
                    if (biomeIndex >= 0 && biomeMap[x, y] != biomeIndex)
                        continue;

                    var randomTexture = textureIDs[Random.Range(0, textureIDs.Count)];
                    var terrainLayer = procGenManager.GetLayerForTexture(randomTexture);

                    alphaMaps[x, y, terrainLayer] = strength;
                }
            }
        }
    }
}