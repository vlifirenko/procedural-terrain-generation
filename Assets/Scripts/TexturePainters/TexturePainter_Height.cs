using System.Collections.Generic;
using PTG.Model;
using PTG.Model.Config;
using UnityEngine;

namespace PTG.TexturePainters
{
    public class TexturePainter_Height : BaseTexturePainter
    {
        [SerializeField] private TextureConfig texture;
        [SerializeField] private float startHeight;
        [SerializeField] private float endHeight;
        [SerializeField] private AnimationCurve intensity;
        [SerializeField] private bool suppressOtherTextures;
        [SerializeField] private AnimationCurve suppressionIntensity;

        public override void Execute(ProcGenManager procGenManager, int mapResolution, float[,] heightMap, Vector3 heightmapScale,
            float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1,
            BiomeConfig biome = null)
        {
            var textureLayer = procGenManager.GetLayerForTexture(texture);

            for (var y = 0; y < alphaMapResolution; y++)
            {
                var heightMapY = Mathf.FloorToInt((float) y * mapResolution / alphaMapResolution);
                var heightMapStart = startHeight / heightmapScale.y;
                var heightMapEnd = endHeight / heightmapScale.y;
                var heightMapRangeInverse = 1f / (heightMapEnd - heightMapStart);
                var numAlphaMaps = alphaMaps.GetLength(2);

                for (var x = 0; x < alphaMapResolution; x++)
                {
                    var heightMapX = Mathf.FloorToInt((float) x * mapResolution / alphaMapResolution);

                    // skip if we have a biome and this is not our biome
                    if (biomeIndex >= 0 && biomeMap[heightMapX, heightMapY] != biomeIndex)
                        continue;

                    var height = heightMap[heightMapX, heightMapY];
                    if (height < heightMapStart || height > heightMapEnd)
                        continue;

                    var heightPercentage = (height - heightMapStart) * heightMapRangeInverse;
                    alphaMaps[x, y, textureLayer] = strength * intensity.Evaluate(heightPercentage);

                    if (suppressOtherTextures)
                    {
                        var suppression = suppressionIntensity.Evaluate(heightPercentage);

                        for (var i = 0; i < numAlphaMaps; i++)
                        {
                            if (i == textureLayer)
                                continue;

                            alphaMaps[x, y, i] *= suppression;
                        }
                    }
                }
            }
        }

        public override List<TextureConfig> RetrieveTextures() => new List<TextureConfig> {texture};
    }
}