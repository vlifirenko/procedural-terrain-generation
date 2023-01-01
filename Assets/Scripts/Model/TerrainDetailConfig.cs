using System;
using UnityEngine;

namespace PTG.Model
{
    [Serializable]
    public class TerrainDetailConfig
    {
        [Header("Grass Billboard Configuration")]
        public Texture2D billboardTexture;
        public Color healthyColor = new Color(67f / 255f, 83f / 85f, 14f / 85f, 1f);
        public Color dryColor = new Color(41f / 51f, 188f / 255f, 26f / 255f, 1f);

        [Header("Detail Mesh Configuration")]
        public GameObject detailPrefab;

        [Header("Common Configuration")]
        public float minWidth = 1f;
        public float maxWidth = 2f;
        public float minHeight = 1f;
        public float maxHeight = 2f;

        public int noiseSeed = 0;
        public float noiseSpread = 0.1f;
        [Range(0f, 1f)] public float holeEdgePadding;
    }
}