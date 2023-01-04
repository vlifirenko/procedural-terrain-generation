using System;
using UnityEngine;

namespace PTG.Model
{
    [Serializable]
    public class RandomDetailPainterConfig
    {
        public TerrainDetailConfig detailToPaint;
        [Range(0f, 1f)] public float intensityModifier = 1f;
        public float noiseScale;
        [Range(0f, 1f)] public float noiseThreshold;
    }
}