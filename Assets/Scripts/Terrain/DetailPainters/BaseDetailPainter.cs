using System.Collections.Generic;
using PTG.Model;
using PTG.Model.Config;
using UnityEngine;

namespace PTG.Terrain.DetailPainters
{
    public abstract class BaseDetailPainter : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float strength = 1f;

        public virtual void Execute(ProcGenManager p0, int mapResolution, float[,] heightMap, Vector3 terrainDataHeightmapScale,
            float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, List<int[,]> detailLayerMaps,
            int detailMapResolution, int maxDetailItems, byte[,] biomeMap = null, int i = -1, BiomeConfig biome = null)
        {
            Debug.LogError($"No implementation of Execute function for {gameObject.name}");
        }

        public virtual List<TerrainDetailConfig> RetrieveTerrainDetails()
        {
            return null;
        }
    }
}