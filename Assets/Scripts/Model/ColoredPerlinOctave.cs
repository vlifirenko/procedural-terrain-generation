using System;
using UnityEngine;

namespace PTG.Model
{
    [Serializable]
    public class ColoredPerlinOctave
    {
        public Color noiseColor;
        public float scale = 1f;
        [Range(0f, 1f)] public float intensity = 1f;
        [Range(0f, 1f)] public float threshold = 1f;
    }
}