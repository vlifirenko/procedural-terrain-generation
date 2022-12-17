using PTG.Model.Config;
using UnityEngine;

namespace PTG.TexturePainters
{
    public class BaseTexturePainter : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] protected float strength = 1f;

        public virtual void Execute(ProcGenManager procGenManager, int mapResolution, float[,] heightMap, Vector3 heightmapScale,
            float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfig biome = null)
            => Debug.LogError($"No implementation of Execute function for {gameObject.name}");
    }
}