using System;
using UnityEngine;

namespace PTG.Model
{
    [Serializable]
    public class TerrainDetailConfig : IEquatable<TerrainDetailConfig>
    {
        [Header("Grass Billboard Configuration")]
        public Texture2D billboardTexture;

        public Color healthyColor = new Color(67f / 255f, 83f / 85f, 14f / 85f, 1f);
        public Color dryColor = new Color(41f / 51f, 188f / 255f, 26f / 255f, 1f);

        [Header("Detail Mesh Configuration")] public GameObject detailPrefab;

        [Header("Common Configuration")] public float minWidth = 1f;
        public float maxWidth = 2f;
        public float minHeight = 1f;
        public float maxHeight = 2f;

        public int noiseSeed = 0;
        public float noiseSpread = 0.1f;
        [Range(0f, 1f)] public float holeEdgePadding;

        public bool Equals(TerrainDetailConfig other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(billboardTexture, other.billboardTexture) && healthyColor.Equals(other.healthyColor) &&
                   dryColor.Equals(other.dryColor) && Equals(detailPrefab, other.detailPrefab) &&
                   minWidth.Equals(other.minWidth) && maxWidth.Equals(other.maxWidth) && minHeight.Equals(other.minHeight) &&
                   maxHeight.Equals(other.maxHeight) && noiseSeed == other.noiseSeed && noiseSpread.Equals(other.noiseSpread) &&
                   holeEdgePadding.Equals(other.holeEdgePadding);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TerrainDetailConfig) obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(billboardTexture);
            hashCode.Add(healthyColor);
            hashCode.Add(dryColor);
            hashCode.Add(detailPrefab);
            hashCode.Add(minWidth);
            hashCode.Add(maxWidth);
            hashCode.Add(minHeight);
            hashCode.Add(maxHeight);
            hashCode.Add(noiseSeed);
            hashCode.Add(noiseSpread);
            hashCode.Add(holeEdgePadding);
            return hashCode.ToHashCode();
        }
    }
}