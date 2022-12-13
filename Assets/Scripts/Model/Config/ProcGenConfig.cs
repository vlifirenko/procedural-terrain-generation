﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PTG.Model.Config
{
    [CreateAssetMenu(fileName = "ProcGen Config", menuName = "Procedural Generation/ProcGen Config", order = -1)]
    public class ProcGenConfig : ScriptableObject
    {
        public List<BiomeConfigItem> biomes;
        [Range(0f, 1f)] public float biomeSeedPointDensity = 0.1f;

        public int NumBiomes => biomes.Count;
        public float TotalWeight => biomes.Sum(biome => biome.weight);
    }
}