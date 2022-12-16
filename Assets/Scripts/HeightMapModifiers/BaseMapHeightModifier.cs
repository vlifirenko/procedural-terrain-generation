using PTG.Model.Config;
using UnityEngine;

namespace PTG.HeightMapModifiers
{
    public class BaseMapHeightModifier : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] protected float strength = 1f;

        public virtual void Execute(int mapResolution, float[,] heightMap, Vector3 heightmapScale, byte[,] biomeMap = null,
            int biomeIndex = -1, BiomeConfig biome = null)
            => Debug.LogError($"No implementation of Execute function for {gameObject.name}");
    }
}