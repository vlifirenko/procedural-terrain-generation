using PTG.Model.Config;
using UnityEngine;

namespace PTG.HeightMapModifiers
{
    public class HeightMapModifier_SetValue : BaseMapHeightModifier
    {
        [SerializeField] private float targetHeight;

        public override void Execute(int mapResolution, float[,] heightMap, Vector3 heightMapScale, byte[,] biomeMap = null,
            int biomeIndex = -1, BiomeConfig biome = null)
        {
            for (var y = 0; y < mapResolution; y++)
            {
                for (var x = 0; x < mapResolution; x++)
                {
                    if (biomeIndex >= 0 && biomeMap[x, y] != biomeIndex)
                        continue;

                    var newHeight = targetHeight / heightMapScale.y;
                    heightMap[x, y] = Mathf.Lerp(heightMap[x, y], newHeight, strength);
                }
            }
        }
    }
}