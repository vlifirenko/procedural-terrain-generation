using System.Collections.Generic;
using System.Linq;
using PTG.Model.Enum;
using UnityEngine;
using UnityEngine.Serialization;

namespace PTG.Model.Config
{
    [CreateAssetMenu(fileName = "ProcGen Config", menuName = "Procedural Generation/ProcGen Config", order = -1)]
    public class ProcGenConfig : ScriptableObject
    {
        public List<BiomeConfigItem> biomes;
        [Range(0f, 1f)] public float biomeSeedPointDensity = 0.1f;
        [FormerlySerializedAs("resolution")] [FormerlySerializedAs("biomeMapBaseResolution")] public ETerrainResolution terrainResolution = ETerrainResolution.Size_64x64;
        
        public GameObject InitialHeightModifier;
        public GameObject HeightPostProcessingModifier;
        public GameObject paintingPostProcessingModifier;
        public GameObject detailPaintingPostProcessingModifier;

        public float waterHeight = 15f;

        public int NumBiomes => biomes.Count;
        public float TotalWeight => biomes.Sum(biome => biome.weight);
    }
}