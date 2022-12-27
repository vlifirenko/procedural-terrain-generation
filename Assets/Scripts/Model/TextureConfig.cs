using System;
using UnityEngine;

namespace PTG.Model
{
    [Serializable]
    public class TextureConfig : IEquatable<TextureConfig>
    {
        public Texture2D diffuse;
        public Texture2D normal;

        public override bool Equals(object obj) => Equals(obj as TextureConfig);

        public bool Equals(TextureConfig other) => other != null && other.diffuse == diffuse && other.normal == normal;

        public override int GetHashCode() => HashCode.Combine(diffuse.GetHashCode(), normal.GetHashCode());
    }
}