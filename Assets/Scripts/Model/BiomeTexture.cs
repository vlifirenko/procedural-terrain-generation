using System;
using UnityEngine;

namespace PTG.Model
{
    [Serializable]
    public class BiomeTexture
    {
        public string uniqueID;
        public Texture2D diffuse;
        public Texture2D normal;
    }
}