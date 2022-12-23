using System;
using UnityEngine;

namespace PTG.Model
{
    [Serializable]
    public class FeatureConfig
    {
        public Texture2D heightMap;
        public float height;
        public int radius;
        public int numToSpawn = 1;
    }
}