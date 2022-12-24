using PTG.Model.Config;
using UnityEngine;

namespace PTG.HeightMapModifiers
{
    public class HeightMapModifier_Random : BaseMapHeightModifier
    {
        [SerializeField] private float heightDelta;

        public override void Execute(ProcGenConfig globalConfig, int mapResolution, float[,] heightMap, Vector3 heightMapScale,
            byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfig biome = null)
        {
            for (var y = 0; y < mapResolution; y++)
            {
                for (var x = 0; x < mapResolution; x++)
                {
                    if (biomeIndex >= 0 && biomeMap[x, y] != biomeIndex)
                        continue;

                    var newHeight = heightMap[x, y] + Random.Range(-heightDelta, heightDelta) / heightMapScale.y;
                    heightMap[x, y] = Mathf.Lerp(heightMap[x, y], newHeight, strength);
                }
            }
        }
    }
}