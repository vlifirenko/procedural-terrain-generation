using System;
using UnityEngine;

namespace PTG.Model.Config
{
    [Serializable]
    public class BiomeConfigItem
    {
        public BiomeConfig biome;
        [Range(0f, 1f)] public float weight = 1f;
    }
}