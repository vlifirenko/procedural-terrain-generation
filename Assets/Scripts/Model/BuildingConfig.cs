using System;
using UnityEngine;

namespace PTG.Model
{
    [Serializable]
    public class BuildingConfig
    {
        public Texture2D heightMap;
        public GameObject prefab;
        public int radius;
        public int numToSpawn = 1;
        public bool hasHeightLimits;
        public float minHeightToSpawn;
        public float maxHeightToSpawn;
        public bool canGoInWater;
        public bool canGoAboveWater = true;
    }
}