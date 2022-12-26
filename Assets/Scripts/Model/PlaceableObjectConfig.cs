using System;
using System.Collections.Generic;
using UnityEngine;

namespace PTG.Model
{
    [Serializable]
    public class PlaceableObjectConfig
    {
        public bool hasHeightLimits;
        public float minHeightToSpawn;
        public float maxHeightToSpawn;
        public bool canGoInWater;
        public bool canGoAboveWater = true;
        [Range(0f, 1f)] public float weighting = 1f;
        public List<GameObject> prefabs;

        public float NormalisedWeighting { get; set; }
    }
}