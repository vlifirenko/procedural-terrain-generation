using System;
using PTG.Model.Config;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PTG.ObjectPlacers
{
    public class ObjectPlacer_Random : BaseObjectPlacer
    {
        public override void Execute(ProcGenConfig globalConfig, Transform objectRoot, int mapResolution, float[,] heightMap,
            Vector3 heightMapScale,
            float[,] slopeMap, float[,,] alphaMaps, int alphaMapResolution, byte[,] biomeMap = null, int biomeIndex = -1,
            BiomeConfig biome = null)
        {
            base.Execute(globalConfig, objectRoot, mapResolution, heightMap, heightMapScale, slopeMap, alphaMaps,
                alphaMapResolution, biomeMap, biomeIndex, biome);

            var candidateLocations =
                GetAllLocationsForBiome(globalConfig, mapResolution, heightMap, heightMapScale, biomeMap, biomeIndex);

            ExecuteSimpleSpawning(globalConfig, objectRoot, candidateLocations);
        }
    }
}