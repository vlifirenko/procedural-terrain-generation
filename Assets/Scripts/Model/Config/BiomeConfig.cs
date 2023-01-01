using System.Collections.Generic;
using PTG.Terrain.DetailPainters;
using PTG.Terrain.TexturePainters;
using UnityEngine;

namespace PTG.Model.Config
{
    [CreateAssetMenu(fileName = "Biome Config", menuName = "Procedural Generation/Biome Config", order = -1)]
    public class BiomeConfig : ScriptableObject
    {
        public string Name;

        [Range(0f, 1f)] public float minIntensity = 0.5f;
        [Range(0f, 1f)] public float maxIntensity = 1f;

        [Range(0f, 1f)] public float minDecayRate = 0.01f;
        [Range(0f, 1f)] public float maxDecayRate = 0.02f;

        public GameObject heightModifier;
        public GameObject terrainPainter;
        public GameObject objectPlacer;
        public GameObject detailPainter;

        public List<TextureConfig> RetrieveTextures()
        {
            if (terrainPainter == null)
                return null;

            var allTextures = new List<TextureConfig>();
            var allPainters = terrainPainter.GetComponents<BaseTexturePainter>();

            foreach (var painter in allPainters)
            {
                var painterTextures = painter.RetrieveTextures();

                if (painterTextures == null || painterTextures.Count == 0)
                    continue;

                allTextures.AddRange(painterTextures);
            }

            return allTextures;
        }
        
        public List<TerrainDetailConfig> RetrieveTerrainDetails()
        {
            if (detailPainter == null)
                return null;

            var allTextures = new List<TerrainDetailConfig>();
            var allPainters = terrainPainter.GetComponents<BaseDetailPainter>();

            foreach (var painter in allPainters)
            {
                var terrainDetails = painter.RetrieveTerrainDetails();

                if (terrainDetails == null || terrainDetails.Count == 0)
                    continue;

                allTextures.AddRange(terrainDetails);
            }

            return allTextures;
        }
    }
}