using PTG.Model.Config;
using UnityEngine;

namespace PTG.Terrain.HeightMapModifiers
{
    public class BaseMapHeightModifier : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] protected float strength = 1f;

        public virtual void Execute(ProcGenConfig globalConfig, int mapResolution, float[,] heightMap, Vector3 heightMapScale,
            byte[,] biomeMap = null, int biomeIndex = -1, BiomeConfig biome = null)
            => Debug.LogError($"No implementation of Execute function for {gameObject.name}");
    }
}